using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;
using RS.Evaluation;
using RS.Data.Utility;
using System.Threading.Tasks;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    ///  computer2009-Koren
    ///  "Matrix factorization techniques for recommender systems"
    /// </summary>
    public class MatrixFactorization
    {
        protected int p = 0;   // Number of Users
        protected int q = 0;   // Number of Items
        protected int f = 10;  // Number of features

        public double[,] P = null;  // Matrix consists of user features
        public double[,] Q = null;  // Matrix consists of item features

        public MatrixFactorization() { }

        public MatrixFactorization(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            InitializeModel(p, q, f, fillMethod);
        }

        public virtual void InitializeModel(int p, int q, int f, string fillMethod = "uniform_df")
        {
            this.p = p;
            this.q = q;
            this.f = f;

            if (fillMethod == "uniform_df")
            {
                P = MathUtility.RandomUniform(p, f, 1.0 / Math.Sqrt(f));
                Q = MathUtility.RandomUniform(q, f, 1.0 / Math.Sqrt(f));
            }
            else if (fillMethod == "gaussian")
            {
                P = MathUtility.RandomGaussian(p, f, 0, 1);
                Q = MathUtility.RandomGaussian(q, f, 0, 1);
            }
            else if (fillMethod == "uniform")
            {
                P = MathUtility.RandomUniform(p, f);
                Q = MathUtility.RandomUniform(q, f);
            }
        }

        public virtual double Predict(int userId, int itemId)
        {
            double _r = 0.0;
            for (int i = 0; i < f; i++)
            {
                _r += P[userId, i] * Q[itemId, i];
            }
            return _r;
        }

        protected virtual double Loss(List<Rating> ratings, double lambda)
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
                    sum_p_i += P[r.UserId, i] * P[r.UserId, i];
                    sum_q_j += Q[r.ItemId, i] * Q[r.ItemId, i];
                }
                loss += lambda * 0.5 * (sum_p_i + sum_q_j);
            }
            return loss;
        }

        public Tuple<double, double> EvaluateMaeRmse(List<Rating> ratings, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            double mae = 0.0;
            double rmse = 0.0;

            foreach (Rating r in ratings)
            {
                double pui = Predict(r.UserId, r.ItemId);

                if (pui < mimimumRating)
                {
                    pui = mimimumRating;
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

        protected void PrintParameters(List<Rating> train, List<Rating> test = null, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test == null ? 0 : test.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("mimimumRating,{0}", mimimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        public void TrySGD(List<Rating> train, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, null, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            Console.WriteLine("epoch,loss,train:mae,train:rmse");
            double loss = Loss(train, lambda);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach (Rating r in train)
                {
                    double pui = Predict(r.UserId, r.ItemId);
                    if (pui < mimimumRating)
                    {
                        pui = mimimumRating;
                    }
                    else if (pui > maximumRating)
                    {
                        pui = maximumRating;
                    }

                    double eui = r.Score - pui;
                    for (int i = 0; i < f; i++)
                    {
                        Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda * Q[r.ItemId, i]);
                        P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda * P[r.UserId, i]);
                    }
                }

                double lastLoss = Loss(train, lambda);
                var eval = EvaluateMaeRmse(train);
                Console.WriteLine("{0},{1},{2},{3}", epoch, lastLoss, eval.Item1, eval.Item2);

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

        public void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            Console.WriteLine("epoch,loss,test:mae,test:rmse");
            double loss = Loss(test, lambda);

            for (int iter = 0; iter < epochs; iter++)
            {
                foreach (Rating r in train)
                {
                    double pui = Predict(r.UserId, r.ItemId);
                    double eui = r.Score - pui;
                    for (int i = 0; i < f; i++)
                    {
                        Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda * Q[r.ItemId, i]);
                        P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda * P[r.UserId, i]);
                    }
                }

                double lastLoss = Loss(test, lambda);
                var eval = EvaluateMaeRmse(test, mimimumRating, maximumRating);
                Console.WriteLine("{0},{1},{2},{3}", iter + 1, lastLoss, eval.Item1, eval.Item2);

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
        protected List<Rating> GetRecommendations(MyTable ratingTable, int N = 10, bool multiThread = false)
        {
            List<Rating> recommendedItems = new List<Rating>();
            ArrayList list = ratingTable.GetSubKeyList();

            if (multiThread)
            {
                int[] mainKeys = new int[ratingTable.Keys.Count];
                ratingTable.Keys.CopyTo(mainKeys, 0);

                Parallel.ForEach(mainKeys, userId =>
                {
                    Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                    List<Rating> predictedRatings = new List<Rating>();
                    foreach (int itemId in list)
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
            }
            else
            {
                foreach (int userId in ratingTable.Keys)
                {
                    Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                    List<Rating> predictedRatings = new List<Rating>();
                    foreach (int itemId in list)
                    {
                        if (!Nu.ContainsKey(itemId))
                        {
                            double p = Predict(userId, itemId);
                            predictedRatings.Add(new Rating(userId, itemId, p));
                        }
                    }
                    List<Rating> sortedLi = predictedRatings.OrderByDescending(r => r.Score).ToList();
                    recommendedItems.AddRange(sortedLi.GetRange(0, Math.Min(sortedLi.Count, N)));
                }
            }
            return recommendedItems;
        }

        public virtual void TrySGDForTopN(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            Console.WriteLine("epoch#train:loss,N,P,R,Coverage,Popularity,MAP");
            double loss = Loss(train, lambda);
            MyTable ratingTable = Tools.GetRatingTable(train);
            int[] K = { 1, 5, 10, 15, 20, 25, 30 };  // recommdation list

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach (Rating r in train)
                {
                    double pui = Predict(r.UserId, r.ItemId);
                    double eui = r.Score - pui;
                    for (int i = 0; i < f; i++)
                    {
                        Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda * Q[r.ItemId, i]);
                        P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda * P[r.UserId, i]);
                    }
                }

                double lastLoss = Loss(train, lambda);
                if (epoch % 2 == 0)
                {
                    Console.Write("{0}#{1}", epoch, lastLoss);  
                    List<Rating> recommendations = GetRecommendations(ratingTable, K[K.Length - 1], true);   // note that, the max K
                    foreach (int k in K)
                    {
                        Console.Write(",{0}", k);
                        List<Rating> subset = Tools.GetSubset(recommendations, k);
                        var pr = Metrics.PrecisionAndRecall(subset, test);
                        var cp = Metrics.CoverageAndPopularity(subset, train);
                        var map = Metrics.MAP(subset, test, k);
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

        public static void Example()
        {
            List<Rating> ratings = new List<Rating>();

            ratings.Add(new Rating(1, 1, 5));
            ratings.Add(new Rating(2, 1, 4));
            ratings.Add(new Rating(3, 1, 1));
            ratings.Add(new Rating(4, 1, 1));
            ratings.Add(new Rating(1, 2, 3));
            ratings.Add(new Rating(3, 2, 1));
            ratings.Add(new Rating(5, 2, 1));
            ratings.Add(new Rating(5, 3, 5));
            ratings.Add(new Rating(1, 4, 1));
            ratings.Add(new Rating(2, 4, 1));
            ratings.Add(new Rating(3, 4, 5));
            ratings.Add(new Rating(4, 4, 4));
            ratings.Add(new Rating(5, 4, 4));

            MatrixFactorization f = new MatrixFactorization(6, 5, 4);
            f.TrySGD(ratings, 500);

            List<Rating> predicts = new List<Rating>();

            predicts.Add(new Rating(5, 1, f.Predict(5, 1)));
            predicts.Add(new Rating(2, 2, f.Predict(2, 2)));
            predicts.Add(new Rating(4, 2, f.Predict(4, 2)));
            predicts.Add(new Rating(1, 3, f.Predict(1, 3)));
            predicts.Add(new Rating(2, 3, f.Predict(2, 3)));
            predicts.Add(new Rating(3, 3, f.Predict(3, 3)));
            predicts.Add(new Rating(4, 3, f.Predict(4, 3)));

            foreach (Rating r in predicts)
            {
                Console.WriteLine("{0},{1},{2}", r.UserId, r.ItemId, r.Score);
            }
        }
    }
}
