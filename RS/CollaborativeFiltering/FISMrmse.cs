using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;
using RS.Core;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// KDD2013-p659-Kabbur
    /// FISM: Factored Item Similarity Models for Top-N Recommender Systems
    /// video http://slideplayer.com/slide/9347453/
    /// </summary>
    public class FISMrmse
    {
        protected int p = 0;   // Number of Users
        protected int q = 0;   // Number of Items
        protected int f = 10;  // Number of features

        // $$P * Q^T$$ denotes item-item similarity matrix
        public double[,] P { get; protected set; }  // Matrix consists of latent item features, left side
        public double[,] Q { get; protected set; }  // Matrix consists of latent item features, right side

        public double[] bu { get; protected set; }  // user biases
        public double[] bi { get; protected set; }  // item biases

        protected double[,] X { get; set; }   // each row in this matrix presents the weighted sum of item features in P.


        public FISMrmse() { }

        public FISMrmse(int p, int q, int f = 10)
        {
            InitializeModel(p, q, f);
        }

        public virtual void InitializeModel(int p, int q, int f)
        {
            this.p = p;
            this.q = q;
            this.f = f;

            bu = new double[p];
            bi = new double[q];

            P = MathUtility.RandomUniform(q, f, -0.001, 0.001); // latent item matrix
            Q = MathUtility.RandomUniform(q, f, -0.001, 0.001); // latent item matrix
            X = new double[p, f];
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
                _r += X[userId, i] * Q[itemId, i];
            }
            return _r + bu[userId] + bi[itemId];
        }

        /// <summary>
        /// update x for each user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="neighbors"></param>
        /// <param name="excludeItemId"></param>
        /// <param name="factor"></param>
        protected void UpdateX(int userId, List<Rating> neighbors, int excludeItemId, double factor)
        {
            foreach (Rating r in neighbors)
            {
                if (r.ItemId != excludeItemId)
                {
                    for (int i = 0; i < f; i++)
                    {
                        X[userId, i] += P[r.ItemId, i];
                    }
                }
            }
            for (int i = 0; i < f; i++)
            {
                X[userId, i] *= factor;
            }
        }

        /// <summary>
        /// Loss function for FISMrmse, see Equation
        /// </summary>
        /// <param name="ratings">entries of R, both rated and unrated</param>
        /// <param name="beta">regularization parameter for P and Q</param>
        /// <param name="lambda">regularization parameter for bu</param>
        /// <param name="gamma">regularization parameter for bi</param>
        /// <returns></returns>
        protected virtual double Loss(List<Rating> ratings, double beta, double lambda, double gamma)
        {
            double loss = 0.0;
            foreach (Rating r in ratings)
            {
                double eui = r.Score - Predict(r.UserId, r.ItemId);
                loss += eui * eui;

                double sum_p_i = 0.0;
                double sum_q_j = 0.0;

                for (int i = 0; i < f; i++)
                {
                    sum_p_i += P[r.ItemId, i] * P[r.ItemId, i];
                    sum_q_j += Q[r.ItemId, i] * Q[r.ItemId, i];
                }

                loss += beta * (sum_p_i + sum_q_j);
                loss += lambda * (bu[r.UserId] * bu[r.UserId]);
                loss += gamma * (bi[r.ItemId] * bi[r.ItemId]);
            }
            loss *= 0.5;
            return loss;
        }


        protected Tuple<double, double> EvaluateMaeRmse(List<Rating> ratings, double minimumRating = 1.0, double maximumRating = 5.0)
        {
            double mae = 0.0;
            double rmse = 0;

            foreach (Rating r in ratings)
            {
                double pui = Predict(r.UserId, r.ItemId);
                if (pui < minimumRating)
                {
                    pui = minimumRating;
                }
                else if (pui > maximumRating)
                {
                    pui = maximumRating;
                }
                double eui = r.Score - pui;

                mae += Math.Abs(eui);
                rmse += eui * eui;
            }

            if (ratings.Count > 0)
            {
                mae /= ratings.Count;
                rmse = Math.Sqrt(rmse / ratings.Count);
            }
            return Tuple.Create(mae, rmse);
        }

        protected void PrintParameters(List<Rating> train, List<Rating> test, int epochs, int rho,
            double yita, double decay, double alpha, double beta, double lambda, double gamma, 
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
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("minimumRating,{0}", minimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        /// <summary>
        /// See Algorithm 1. 
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
        public void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, int rho = 3, 
            double yita = 0.001, double decay = 1.0, double alpha = 0.5, double beta = 2e-4, double lambda = 0.01, double gamma = 0.01)
        {
            var sampledRatings = Tools.RandomSelectNegativeSamples(train, rho, false);
            var scoreBounds = Tools.GetMinAndMaxScore(sampledRatings);
            double loss = Loss(sampledRatings, beta, lambda, gamma);

            //test = Tools.ConvertToBinary(test);
            MyTable ratingTable = Tools.GetRatingTable(train);
            int[] Ns = { 1, 5, 10, 15, 20, 25, 30 };

            PrintParameters(sampledRatings, test, epochs, rho, yita, decay, 
                alpha, beta, lambda, gamma, scoreBounds.Item1, scoreBounds.Item2);
            Console.WriteLine("epoch,loss#train,mae#test,rmse#test");

            for (int epoch = 1; epoch <= epochs; epoch++)
            { 
                sampledRatings = Tools.RandomSelectNegativeSamples(train, rho, false);
                var userItemsTable = Tools.GetUserItemsTable(sampledRatings);

                foreach (Rating r in sampledRatings)
                {
                    var neighbors = (List<Rating>)userItemsTable[r.UserId];

                    double factorOfX = Math.Pow(neighbors.Count - 1, -alpha);
                    UpdateX(r.UserId, neighbors, r.ItemId, factorOfX);

                    double pui = Predict(r.UserId, r.ItemId);
                    double eui = r.Score - pui;

                    bu[r.UserId] += yita * (eui - lambda * bu[r.UserId]);
                    bi[r.ItemId] += yita * (eui - gamma  * bi[r.ItemId]);

                    for (int i = 0; i < f; i++)
                    {
                        Q[r.ItemId, i] += yita * (eui * X[r.UserId, i] - beta * Q[r.ItemId, i]);
                    }

                    foreach(Rating j in neighbors)
                    {
                        if(j.ItemId != r.ItemId)
                        {
                            for (int i = 0; i < f; i++)
                            {
                                P[j.ItemId, i] += yita * (eui * factorOfX * Q[r.ItemId, i] - beta * P[j.ItemId, i]);
                            }
                        }
                    }                  
                }

                
                double lastLoss = Loss(sampledRatings, beta, lambda, gamma);

                // evaluate MAE & RMSE 
                //var eval = EvaluateMaeRmse(test, scoreBounds.Item1, scoreBounds.Item2);
                //Console.WriteLine("{0},{1},{2},{3}", epoch, lastLoss, eval.Item1, eval.Item2);

                if (epoch % 1 == 0 && epoch >= 1)
                {
                    //Console.Write("{0}#{1}", epoch, lastLoss);
                    Console.Write("{0}", epoch);
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
        ///  Algorithm 1, optimized by Zhijin Wang, 2017.12.27 
        /// Learning FISMrmse by using SGD.
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="epochs">maximum epochs</param>
        /// <param name="rho">#negative ratings / #postive ratings in sample negative samples</param>
        /// <param name="yita">learning rate</param>
        /// <param name="decay">decay for learning rate</param>
        /// <param name="alpha">power of the factor of x, neighborhood agreement constant</param>
        /// <param name="beta">regularization parameter of P and Q</param>
        /// <param name="lambda">regularization parameter of bu</param>
        /// <param name="gamma">regularization parameter of bi</param>
        public void TrySGDv2(List<Rating> train, List<Rating> test, int epochs = 100, int rho = 3,
            double yita = 0.001, double decay = 1.0, double alpha = 0.5, double beta = 2e-4, double lambda = 1e-4, double gamma = 1e-4)
        {
            var sampledRatings = Tools.RandomSelectNegativeSamples(train, rho, false);
            var scoreBounds = Tools.GetMinAndMaxScore(sampledRatings);
            double loss = Loss(sampledRatings, beta, lambda, gamma);
            var userItemsTable = Tools.GetUserItemsTable(sampledRatings);

            PrintParameters(sampledRatings, test, epochs, rho, yita, decay,
                alpha, beta, lambda, gamma, scoreBounds.Item1, scoreBounds.Item2);
            Console.WriteLine("epoch#loss(train),N,P,R,Coverage,Popularity,MAP");

            MyTable ratingTable = Tools.GetRatingTable(train);
            int[] Ns = { 1, 5, 10, 15, 20, 25, 30 }; 

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach(int userId in userItemsTable.Keys)
                {
                    var neighbors = (List<Rating>)userItemsTable[userId];
                    double factorOfX = Math.Pow(neighbors.Count - 1, -alpha);

                    foreach (Rating r in neighbors)
                    {
                        UpdateX(r.UserId, neighbors, r.ItemId, factorOfX);

                        double pui = Predict(r.UserId, r.ItemId);
                        double eui = r.Score - pui;
                        bu[r.UserId] += yita * (eui - lambda * bu[r.UserId]);
                        bi[r.ItemId] += yita * (eui - gamma  * bi[r.ItemId]);

                        for (int i = 0; i < f; i++)
                        {
                            Q[r.ItemId, i] += yita * (eui * X[r.UserId, i] - beta * Q[r.ItemId, i]);
                            P[r.ItemId, i] += yita * (eui * factorOfX * Q[r.ItemId, i] - beta * P[r.ItemId, i]);
                        }
                        //foreach (Rating j in neighbors)
                        //{
                        //    if (j.ItemId != r.ItemId)
                        //    {
                        //        for (int i = 0; i < f; i++)
                        //        {
                        //            P[j.ItemId, i] += yita * (eui * factorOfX * Q[r.ItemId, i] - beta * P[j.ItemId, i]);
                        //        }
                        //    }
                        //}
                    }                 
                }

                double lastLoss = Loss(sampledRatings, beta, lambda, gamma);
                if (epoch % 1 == 0 && epoch >= 1)
                {
                    //Console.Write("{0}#{1}", epoch, lastLoss);
                    Console.Write("{0}", epoch);
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
