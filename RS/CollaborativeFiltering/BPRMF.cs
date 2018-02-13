using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.Core;
using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// UAI2009, p452, Stefen Rendle, BPR: Bayesian Personalized Ranking from Implicit Feedback
    /// </summary>
    public class BPRMF : MatrixFactorization
    {
        /// <summary>item bias terms</summary>
        public double[] bi { get; protected set; }

        public BPRMF() { }

        public BPRMF(int p, int q, int f = 10, string fillMethod = "gaussian")
        {
            InitializeModel(p, q, f, fillMethod);
        }

        public override void InitializeModel(int p, int q, int f, string fillMethod = "gaussian")
        {
            base.InitializeModel(p, q, f, fillMethod);
            bi = new double[q];
        }

        protected Tuple<int, int, int>  SampleItemPair(int userId, Hashtable itemsTable, int[] itemIds, Core.Random random)
        {
            // an item rated by a user
            int itemId = itemIds[random.Next(itemIds.Length)];
            while (!itemsTable.ContainsKey(itemId))
            {
                itemId = itemIds[random.Next(itemIds.Length)];
            }

            // an item which has not been rated by a user
            int otherItemId = itemIds[random.Next(itemIds.Length)];
            while (itemsTable.ContainsKey(otherItemId))
            {
                otherItemId = itemIds[random.Next(itemIds.Length)];
            }
            return Tuple.Create(userId, itemId, otherItemId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ratingTable"></param>
        /// <param name="ratio"></param>
        /// <returns>list of triples: user id - item id - other item id</returns>
        protected List<Tuple<int, int, int>> SampleTriples(MyTable ratingTable, int ratio = 100)
        {
            int[] userIds = ratingTable.GetMainKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            int[] itemIds = ratingTable.GetSubKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            List<Tuple<int, int, int>> list = new List<Tuple<int, int, int>>();

            var random = Core.Random.GetInstance();         // need to be a static member?
            for (int i = 0; i < userIds.Length * ratio; i++)
            {
                // randomly select a user
                int userId = userIds[random.Next(userIds.Length)];
                Hashtable itemsTable = (Hashtable)ratingTable[userId];

                var triple = SampleItemPair(userId, itemsTable, itemIds, random);
                list.Add(triple);
            }
            return list;
        }


        public override double Predict(int userId, int itemId)
        {
            return base.Predict(userId, itemId) + bi[itemId];
        }

        /// <summary>
        /// Objective function.
        /// </summary>
        /// <param name="triples">triples of user - positive item id - negative item id</param>
        /// <param name="lambda">regularization parameter for user factors, positive item factors, negative item factors</param>
        /// <param name="labmda_bias">regularization parameter for bias terms</param>
        /// <returns></returns>
        protected double Loss(List<Tuple<int, int, int>> triples, double lambda = 0.00025, double lambda_bias = 0.00025)
        {
            double rankingLoss = 0;
            double sum_p_u = 0.0;
            double sum_q_i = 0.0;
            double sum_q_j = 0.0;
            double sum_b_i = 0.0;
            double sum_b_j = 0.0;

            foreach (var t in triples)
            {
                double e_uij = Predict(t.Item1, t.Item2) - Predict(t.Item1, t.Item3);
                rankingLoss += MathUtility.Logistic(e_uij);
                for (int i = 0; i < f; i++)
                {
                    sum_p_u += P[t.Item1, i] * P[t.Item1, i];
                    sum_q_i += Q[t.Item2, i] * Q[t.Item2, i];
                    sum_q_j += Q[t.Item3, i] * Q[t.Item3, i];
                }
                sum_b_i += bi[t.Item2] * bi[t.Item2];
                sum_b_j += bi[t.Item3] * bi[t.Item3];
            }
            return rankingLoss + 0.5 * lambda * (sum_p_u + sum_q_i + sum_q_j) + 0.5 * lambda_bias + (sum_b_i + sum_b_j);
        }

        protected int SampleOtherItemId(int userId, Hashtable itemsTable, int[] itemIds, Core.Random random)
        {
            // an item which has not been rated by a user
            int otherItemId = itemIds[random.Next(itemIds.Length)];
            while (itemsTable.ContainsKey(otherItemId))
            {
                otherItemId = itemIds[random.Next(itemIds.Length)];
            }
            return otherItemId;
        }

        protected void UpdateFactors(int userId, int itemId, int otherItemId, double gamma, double lambda, double lambda_bias)
        {
            // 2. Update latent factors according to the stochastic gradient descent update rule
            double e_uij = Predict(userId, itemId) - Predict(userId, otherItemId);
            double one_over_one_plus_ex = MathUtility.Logistic(-e_uij); // 1.0 / (1.0 + Math.Exp(e_uij));

            // adjust bias terms
            bi[itemId] += gamma * (one_over_one_plus_ex - lambda_bias * bi[itemId]);
            bi[otherItemId] += gamma * (-one_over_one_plus_ex - lambda_bias * bi[otherItemId]);

            // adjust latent factors
            for (int i = 0; i < f; i++)
            {
                double w_uf = P[userId, i];
                double h_if = Q[itemId, i];
                double h_jf = Q[otherItemId, i];

                P[userId, i] += gamma * (one_over_one_plus_ex * (h_if - h_jf) - lambda * w_uf);
                Q[itemId, i] += gamma * (one_over_one_plus_ex * w_uf - lambda * h_if);
                Q[otherItemId, i] += gamma * (one_over_one_plus_ex * -w_uf - lambda * h_jf);
            }
        }

        /// <summary>
        /// Iterate over the training data, uniformly sample from users without replacement.
        /// </summary>
        /// <param name="ratings"></param>
        /// <param name="ratingTable"></param>
        /// <param name="gamma"></param>
        /// <param name="lambda"></param>
        /// <param name="lambda_bias"></param>
        protected virtual void IterateWithoutReplacementUniformUser(List<Rating> ratings, MyTable ratingTable, double gamma = 0.01, double lambda = 0.01, double lambda_bias = 0.01)
        {
            int[] userIds = ratingTable.GetMainKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            int[] itemIds = ratingTable.GetSubKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            var random = Core.Random.GetInstance();
            int numberOfRatings = ratings.Count;
            for (int ii = 0; ii < numberOfRatings; ii++)
            {
                // randomly select a user
                int userId = userIds[random.Next(userIds.Length)];
                Hashtable itemsTable = (Hashtable)ratingTable[userId];
                var triple = SampleItemPair(userId, itemsTable, itemIds, random);
                int itemId = triple.Item2;
                int otherItemId = triple.Item3;

                UpdateFactors(userId, itemId, otherItemId, gamma, lambda, lambda_bias);
            }
        }


        /// <summary>
        /// Iterate over the training data, uniformly sample from user-item pairs without replacement.
        /// </summary>
        protected virtual void IterateWithoutReplacementUniformPair(MyTable ratingTable, double gamma = 0.01, double lambda = 0.01, double lambda_bias = 0.01)
        {
            int[] itemIds = ratingTable.GetSubKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            var random = Core.Random.GetInstance();         // need to be a static member?

            foreach (int userId in ratingTable.Keys)
            {
                Hashtable itemsTable = (Hashtable)ratingTable[userId];
                foreach(int itemId in itemsTable.Keys)
                {
                    // 1. sample a negative feedback for each positive feedback
                    int otherItemId = SampleOtherItemId(userId, itemsTable, itemIds, random);

                    UpdateFactors(userId, itemId, otherItemId, gamma, lambda, lambda_bias);
                }
            }
        }

        /// <summary>
        /// Iterate over the training data, uniformly sample from user-item pairs with replacement.
        /// </summary>
        protected virtual void IterateWithReplacementUniformPair(List<Rating> ratings, MyTable ratingTable, double gamma = 0.01, double lambda = 0.01, double lambda_bias = 0.01)
        {
            int[] itemIds = ratingTable.GetSubKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            var random = Core.Random.GetInstance();
            int numberOfRatings = ratings.Count;
            for (int ii = 0; ii < numberOfRatings; ii++)
            {
                Rating r = ratings[random.Next(numberOfRatings)];
                int userId = r.UserId;
                int itemId = r.ItemId;

                Hashtable itemsTable = (Hashtable)ratingTable[userId];
                // 1. sample a negative feedback for each positive feedback
                int otherItemId = SampleOtherItemId(userId, itemsTable, itemIds, random);

                UpdateFactors(userId, itemId, otherItemId, gamma, lambda, lambda_bias);
            }
        }



        protected void PrintParameters(List<Rating> train, List<Rating> test, int epochs, double gamma, double decay,
            double lambda, double lambda_bias)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("lambda_bias,{0}", lambda_bias);
        }

        public void TryTopN(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.05, double decay = 1.0,
            double lambda = 0.00025, double lambda_bias = 0.00025)
        {
            PrintParameters(train, test, epochs, gamma, decay, lambda, lambda_bias);
            var ratingTable = Tools.GetRatingTable(train, true);
            var triples = SampleTriples(ratingTable, 100);  // sampled for computing loss function.
            double loss = Loss(triples, lambda, lambda_bias);
            int[] Ns = { 1, 5, 10, 15, 20, 25, 30 };  

            Console.WriteLine("epoch#train:loss,N,P,R,Coverage,Popularity,MAP");

            for (int e = 1; e <= epochs; e++)
            {
                //IterateWithReplacementUniformPair(ratingTable, gamma, lambda_bias, lambda_bias);
                //IterateWithReplacementUniformPair(train, ratingTable, gamma, lambda_bias, lambda_bias);
                IterateWithoutReplacementUniformUser(train, ratingTable, gamma, lambda_bias, lambda_bias);

                double lastLoss = Loss(triples, lambda, lambda_bias);
                if (e % 1 == 0 && e >= 1)
                {
                    Console.Write("{0}#{1}", e, lastLoss);
                    List<Rating> recommendations = GetRecommendations(ratingTable, Ns[Ns.Length - 1], true);   // note that, the max K
                    foreach (int n in Ns)
                    {
                        Console.Write(",{0}", n);
                        List<Rating> subset = Tools.GetSubset(recommendations, n);
                        var pr = Metrics.PrecisionAndRecall(subset, test);
                        var cp = Metrics.CoverageAndPopularity(subset, train);
                        var map = Metrics.MAP(subset, test, n);
                        Console.WriteLine(",{0},{1},{2},{3},{4}", pr.Item1, pr.Item2, cp.Item1, cp.Item2, map);
                    }
                }

                if (decay != 1.0)
                {
                    gamma *= decay;
                }
                if (lastLoss < loss)
                {
                    loss = lastLoss;
                }
                else
                {
                   // break;
                }
            }
        }
    }
}
