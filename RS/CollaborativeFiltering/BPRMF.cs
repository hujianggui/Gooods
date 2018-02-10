using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.Core;
using RS.Data.Utility;
using RS.DataType;

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

        public BPRMF(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            InitializeModel(p, q, f, fillMethod);
        }

        public override void InitializeModel(int p, int q, int f, string fillMethod = "uniform_df")
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

            var random = Core.Random.GetInstance(); // need to be a static member?
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

        public void TryTopN(List<Rating> train, List<Rating> test, int epochs = 100, double lambda = 0.00025, double lambda_bias = 0.00025)
        {
            var ratingTable = Tools.GetRatingTable(train);
            var triples = SampleTriples(ratingTable, 100);  // sampled for computing loss function.
            double loss = Loss(triples, lambda, lambda_bias);

            for (int e = 1; e <= epochs; e++)
            {
            }
        }
    }
}
