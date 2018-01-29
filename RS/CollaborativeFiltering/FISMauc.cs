using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RS.Core;
using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;

namespace RS.CollaborativeFiltering
{
    public class FISMauc
    {
        protected int p = 0;   // Number of Users
        protected int q = 0;   // Number of Items
        protected int f = 10;  // Number of features

        // $$P * Q^T$$ denotes item-item similarity matrix
        public double[,] P { get; protected set; }  // Matrix consists of latent item features, left side
        public double[,] Q { get; protected set; }  // Matrix consists of latent item features, right side

        public double[] bi { get; protected set; }  // item biases

        protected double[,] T { get; set; }   // each row in this matrix presents the weighted sum of item features in P.


        public FISMauc() { }

        public FISMauc(int p, int q, int f = 10)
        {
            InitializeModel(p, q, f);
        }

        public virtual void InitializeModel(int p, int q, int f)
        {
            this.p = p;
            this.q = q;
            this.f = f;

            bi = new double[q];

            P = MathUtility.RandomUniform(q, f, -0.001, 0.001); // latent item matrix
            Q = MathUtility.RandomUniform(q, f, -0.001, 0.001); // latent item matrix
            T = new double[p, f];
        }

        /// <summary>
        /// The pdf says: $$\hat(r_{ui}) = b_u + b_i + q_i^T x$$, see Equation (7).
        /// </summary>
        /// <param name="userId">user ID</param>
        /// <param name="itemId">item ID</param>
        /// <returns></returns>
        public virtual double Predict(int userId, int itemId)
        {
            double _r = 0.0;
            for (int i = 0; i < f; i++)
            {
                _r += T[userId, i] * Q[itemId, i];
            }
            return _r + bi[itemId];
        }


        /// <summary>
        /// update t for each user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="neighbors"></param>
        /// <param name="excludeItemId"></param>
        /// <param name="factor"></param>
        protected void UpdateT(int userId, List<Rating> neighbors, int excludeItemId, double factor)
        {
            foreach (Rating r in neighbors)
            {
                if (r.ItemId != excludeItemId)
                {
                    for (int i = 0; i < f; i++)
                    {
                        T[userId, i] += P[r.ItemId, i];
                    }
                }
            }
            for (int i = 0; i < f; i++)
            {
                T[userId, i] *= factor;
            }
        }


        protected virtual double Loss(Hashtable classifiedUserItemsTable, double beta, double gamma)
        {
            double loss = 0.0;
            foreach (int userId in classifiedUserItemsTable.Keys)
            {
                var tuple = (Tuple<List<Rating>, List<Rating>>)classifiedUserItemsTable[userId];
                var positives = tuple.Item1;
                var negatives = tuple.Item2;

                foreach (Rating i in positives)
                {
                    double pui = Predict(i.UserId, i.ItemId);
                    foreach (Rating j in negatives)
                    {
                        double puj = Predict(j.UserId, j.ItemId);
                        double eij = (i.Score - j.Score) - (pui - puj);
                        loss += eij * eij;                    
                    }
                    double sum_p_i = 0.0;
                    double sum_q_j = 0.0;

                    for (int index = 0; index < f; index++)
                    {
                        sum_p_i += P[i.ItemId, index] * P[i.ItemId, index];
                        sum_q_j += Q[i.ItemId, index] * Q[i.ItemId, index];
                    }
                    loss += beta * (sum_p_i + sum_q_j);
                    loss += gamma * (bi[i.ItemId] * bi[i.ItemId]);
                }
            }
            loss *= 0.5;
            return loss;
        }

        protected void PrintParameters(List<Rating> train, List<Rating> test, int epochs, int rho,
            double yita, double decay, double alpha, double beta, double gamma,
            double minimumRating, double maximumRating)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("rho,{0}", rho);
            Console.WriteLine("yita,{0}", yita);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("alpha,{0}", alpha);
            Console.WriteLine("beta,{0}", beta);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("minimumRating,{0}", minimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        /// <summary>
        /// Get recommendations based on the trained model?
        /// </summary>
        /// <param name="ratingTable"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        protected List<Rating> GetRecommendations(MyTable ratingTable, int N = 10)
        {
            List<Rating> recommendedItems = new List<Rating>();
            var itemIds = (int[])ratingTable.GetSubKeyArray();
            var userIds = (int[])ratingTable.GetMainKeyArray();

            Parallel.ForEach(userIds, userId =>
            {
                Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                List<Rating> predictedRatings = new List<Rating>();
                foreach (int itemId in itemIds)
                {
                    if (!Nu.ContainsKey(itemId))
                    {
                        double p = Predict(userId, itemId);
                        predictedRatings.Add(new Rating(userId, itemId, p));
                    }
                }
                List<Rating> sortedLi = predictedRatings.OrderByDescending(r => r.Score).ToList();
                lock (recommendedItems)
                {
                    recommendedItems.AddRange(sortedLi.GetRange(0, Math.Min(sortedLi.Count, N)));
                }
            });

            return recommendedItems;
        }

        /// <summary>
        /// See Algorithm 2. 
        /// Learning FISMrmse by using SGD.
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="epochs">maximum epochs</param>
        /// <param name="rho">#negative ratings / #postive ratings in sample negative samples</param>
        /// <param name="yita">learning rate</param>
        /// <param name="decay">decay for learning rate</param>
        /// <param name="alpha">power of the factor of x</param>
        /// <param name="beta">regularization parameter of P and Q</param>
        /// <param name="lambda">regularization parameter of bu</param>
        /// <param name="gamma">regularization parameter of bi</param>
        public void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, int rho = 1,
            double yita = 0.0001, double decay = 1.0, double alpha = 0.5, double beta = 2e-4, double gamma = 1e-4)
        {
            var sampledRatings = Tools.RandomSelectNegativeSamples(train, rho, false);
            var scoreBounds = Tools.GetMinAndMaxScore(sampledRatings);
            var userItemsTable = Tools.GetClassifiedUserItemsTable(sampledRatings);
            double loss = Loss(userItemsTable, beta, gamma);

            PrintParameters(sampledRatings, test, epochs, rho, yita, decay,
                alpha, beta, gamma, scoreBounds.Item1, scoreBounds.Item2);
            Console.WriteLine("epoch#loss(train),N,P,R,Coverage,Popularity,MAP");

            MyTable ratingTable = Tools.GetRatingTable(train);
            int[] Ns = { 1, 5, 10, 15, 20, 25, 30 };

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach (int userId in userItemsTable.Keys)
                {
                    var tuple = (Tuple<List<Rating>, List<Rating>>)userItemsTable[userId];
                    var positives = tuple.Item1;
                    var negatives = tuple.Item2;

                    //Tools.WriteRatings(positives, @"D:\p.csv");
                    //Tools.WriteRatings(negatives, @"D:\n.csv");

                    double factorOfX = Math.Pow(positives.Count - 1, -alpha);

                    foreach (Rating i in positives)
                    {
                        UpdateT(i.UserId, positives, i.ItemId, factorOfX);  // can be optimized
                        double pui = Predict(i.UserId, i.ItemId);

                        double[] x = new double[f]; // what is the problem?

                        foreach (Rating j in negatives)
                        {
                            double puj = Predict(j.UserId, j.ItemId);
                            double eij = (i.Score - j.Score) - (pui - puj);

                            bi[i.ItemId] += yita * (eij - gamma * bi[i.ItemId]);
                            bi[j.ItemId] -= yita * (eij - gamma * bi[j.ItemId]);

                            for (int index = 0; index < f; index++)
                            {
                                Q[i.ItemId, index] += yita * (eij * T[i.UserId, index] - beta * Q[i.ItemId, index]);
                                Q[j.ItemId, index] -= yita * (eij * T[j.UserId, index] - beta * Q[j.ItemId, index]);
                                x[index] += eij * (Q[i.ItemId, index] - Q[j.ItemId, index]);
                            }
                        }

                        //for (int index = 0; index < f; index++)
                        //{
                        //    P[i.ItemId, index] += yita * (factorOfX * x[index] / rho - beta * P[i.ItemId, index]);
                        //}

                        foreach (Rating k in positives)
                        {
                            if (k.ItemId != i.ItemId)
                            {
                                for (int index = 0; index < f; index++)
                                {
                                    P[k.ItemId, index] += yita * (factorOfX * x[index] / rho - beta * P[k.ItemId, index]);
                                }
                            }
                        }
                    }
                }

                double lastLoss = Loss(userItemsTable, beta, gamma);
                if (epoch % 1 == 0 && epoch >= 1)
                {
                    Console.Write("{0}#{1}", epoch, lastLoss);
                    //Console.Write("{0}", epoch);
                    List<Rating> recommendations = GetRecommendations(ratingTable, Ns[Ns.Length - 1]);   // note that, the max K
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
                    break;
                }
            }

        }
    }
}