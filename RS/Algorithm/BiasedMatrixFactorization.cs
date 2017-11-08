using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using RS.DataType;
using RS.Data.Utility;
using RS.Evaluation;


namespace RS.Algorithm
{
    public class BiasedMatrixFactorization 
    {
        protected int p = 0;   // Number of Users
        protected int q = 0;   // Number of Items
        protected int f = 10;  // Number of features

        public double[,] P = null;  // Matrix consists of user features
        public double[,] Q = null;  // Matrix consists of item features
        public double[] bu = null;
        public double[] bi = null;

        public BiasedMatrixFactorization() { }

        public BiasedMatrixFactorization(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            InitializeModel(p, q, f, fillMethod);
        }

        public virtual void InitializeModel(int p, int q, int f, string fillMethod = "uniform_df")
        {
            this.p = p;
            this.q = q;
            this.f = f;

            bu = new double[p];
            bi = new double[q];

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

        public virtual double Predict(int userId, int itemId, double miu)
        {
            double _r = 0.0;
            for (int i = 0; i < f; i++)
            {
                _r += P[userId, i] * Q[itemId, i];
            }
            return _r + bu[userId] + bi[itemId] + miu;
        }

        protected virtual double Loss(List<Rating> ratings, double lambda, double miu)
        {
            double loss = 0.0;
            foreach (Rating r in ratings)
            {
                double eui = r.Score - Predict(r.UserId, r.ItemId, miu);
                loss += eui * eui;

                double sum_p_i = 0.0;
                double sum_q_j = 0.0;

                for (int i = 0; i < f; i++)
                {
                    sum_p_i += P[r.UserId, i] * P[r.UserId, i];
                    sum_q_j += Q[r.ItemId, i] * Q[r.ItemId, i];
                }
                loss += lambda * 0.5 * (sum_p_i + sum_q_j + bu[r.UserId] * bu[r.UserId] + bi[r.ItemId] * bi[r.ItemId]);
            }
            return loss;
        }

        public Tuple<double, double> EvaluateMaeRmse(List<Rating> ratings, double miu, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            double mae = 0.0;
            double rmse = 0;

            foreach (Rating r in ratings)
            {
                double pui = Predict(r.UserId, r.ItemId, miu);
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

        public void PrintParameters(List<Rating> train, List<Rating> test = null, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
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

        public virtual void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            Console.WriteLine("epoch,loss,test:mae,test:rmse");

            double miu = train.AsParallel().Average(r => r.Score);
            double loss = Loss(test, lambda, miu);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach (Rating r in train)
                {
                    double pui = Predict(r.UserId, r.ItemId, miu);
                    double eui = r.Score - pui;
                    bu[r.UserId] += gamma * (eui - lambda * bu[r.UserId]);
                    bi[r.ItemId] += gamma * (eui - lambda * bi[r.ItemId]);

                    for (int i = 0; i < f; i++)
                    {
                        Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda * Q[r.ItemId, i]);
                        P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda * P[r.UserId, i]);
                    }
                }

                double lastLoss = Loss(test, lambda, miu);
                var eval = EvaluateMaeRmse(test, miu, mimimumRating, maximumRating);
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

        protected List<Rating> GetRecommendations(MyTable ratingTable, double miu, int N = 10, bool multiThread = false)
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
                            double p = Predict(userId, itemId, miu);
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
                            double p = Predict(userId, itemId, miu);
                            predictedRatings.Add(new Rating(userId, itemId, p));
                        }
                    }
                    List<Rating> sortedLi = predictedRatings.OrderByDescending(r => r.Score).ToList();
                    recommendedItems.AddRange(sortedLi.GetRange(0, Math.Min(sortedLi.Count, N)));
                }
            }
            return recommendedItems;
        }

        public void TrySGDForTopN(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0)
        {
            PrintParameters(train, test, epochs, gamma, lambda, decay, 0, 1);
            Console.WriteLine("epoch,train:loss,K(Cosine),N,P,R,Coverage,Popularity");

            double miu = train.Average(r => r.Score);
            double loss = Loss(train, lambda, miu);

            int[] K = { 1, 5, 10, 15, 20, 25, 30 };  // recommdation list
            MyTable ratingTable = Tools.GetRatingTable(train);

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                foreach (Rating r in train)
                {
                    double pui = Predict(r.UserId, r.ItemId, miu);
                    double eui = r.Score - pui;
                    for (int i = 0; i < f; i++)
                    {
                        Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda * Q[r.ItemId, i]);
                        P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda * P[r.UserId, i]);
                    }
                }

                double lastLoss = Loss(train, lambda, miu);
                if (epoch % 2 == 0)
                {
                    Console.Write("{0}#{1}", epoch, lastLoss);
                    List<Rating> recommendations = GetRecommendations(ratingTable, miu, K[K.Length - 1], true);   // note that, the max K
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


    }
}
