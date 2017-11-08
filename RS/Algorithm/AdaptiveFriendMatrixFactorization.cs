using System;
using System.Collections;
using System.Collections.Generic;

using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;

namespace RS.Algorithm
{

    public class AdaptiveFriendMatrixFactorization : FriendMatrixFactorization
    {
        public double[] W { get; private set; }   // weights of linked latent factors.

        public AdaptiveFriendMatrixFactorization() { }

        public AdaptiveFriendMatrixFactorization(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            InitializeModel(p, q, f, fillMethod);
        }
        public override void InitializeModel(int p, int q, int f, string fillMethod = "uniform_df")
        {
            base.InitializeModel(p, q, f, fillMethod);
            W = new double[p];
        }

        protected void UpdateX(int uId, List<Link> links)
        {
            for (int i = 0; i < f; i++) // set to 0
            {
                X[uId, i] = 0;
            }

            foreach (Link t in links)
            {
                if (t.To > p)  // if linked user id not exist in dataset
                {
                    continue;
                }
                for (int i = 0; i < f; i++)
                {
                    X[uId, i] += P[t.To, i];
                }
            }
            for (int i = 0; i < f; i++)
            {
                X[uId, i] *= (1.0 / links.Count);   //  (w / Math.Sqrt(friends.Count));
            }
        }

        protected void UpdateX(Hashtable userLinksTable)
        {
            foreach (int uId in userLinksTable.Keys)
            {
                List<Link> links = (List<Link>)userLinksTable[uId];
                UpdateX(uId, links);
            }
        }

        public override double Predict(int uId, int iId)
        {
            double _r = 0.0;
            for (int i = 0; i < f; i++)
            {
                _r += P[uId, i] * (Q[iId, i] + W[uId] * X[uId, i]);
            }
            return _r;
        }

        protected override double Loss(List<Rating> ratings, double lambda)
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
                loss += lambda * 0.5 * (sum_p_i + sum_q_j + W[r.UserId]);
            }
            return loss;
        }

        private void PrintParameters(List<Rating> train, List<Rating> test, List<Link> links, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test == null ? 0 : test.Count);
            Console.WriteLine("links,{0}", links.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("mimimumRating,{0}", mimimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        public void TrySGD(List<Rating> train, List<Rating> test, List<Link> links, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, links, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            Console.WriteLine("epoch,loss,test:mae,test:rmse");
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable userLinksTable = Tools.GetUserLinksTable(links);

            UpdateX(userLinksTable);
            double loss = Loss(train, lambda);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                // UpdateX(userLinksTable, w);
                foreach (int uId in userItemsTable.Keys)
                {
                    if (userLinksTable.ContainsKey(uId))
                    {
                        List<Link> _links = (List<Link>)userLinksTable[uId];
                        UpdateX(uId, _links);
                    }

                    List<Rating> ratings = (List<Rating>)userItemsTable[uId]; // ratings with an UserId
                    foreach (Rating r in ratings)
                    {
                        double pui = Predict(r.UserId, r.ItemId);
                        double eui = r.Score - pui;
                        for (int i = 0; i < f; i++)
                        {
                            P[r.UserId, i] += gamma * (eui * (Q[r.ItemId, i] + X[r.UserId, i]) - lambda * P[r.UserId, i]);
                            Q[r.ItemId, i] += gamma * (eui * (P[r.UserId, i]) - lambda * Q[r.ItemId, i]);
                        }

                        // update W_u
                        double _sum = 0.0;
                        for (int i = 0; i < f; i++)
                        {
                            _sum += X[uId, i] * P[uId, i];
                        }
                        if (_sum != 0)
                        {
                            W[uId] += gamma * (eui * _sum - lambda * W[uId]);
                        }
                    }
                }

                double lastLoss = Loss(test, lambda);
                var eval = EvaluateMaeRmse(test, mimimumRating, maximumRating);
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

        /// <summary>
        /// too long time to run a result 
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="links"></param>
        /// <param name="epochs"></param>
        /// <param name="gamma"></param>
        /// <param name="lambda"></param>
        /// <param name="decay"></param>
        /// <param name="mimimumRating"></param>
        /// <param name="maximumRating"></param>
        public void TrySGDForTopN(List<Rating> train, List<Rating> test, List<Link> links, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, links, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable userLinksTable = Tools.GetUserLinksTable(links);
            MyTable ratingTable = Tools.GetRatingTable(train);
            int[] K = { 1, 5, 10, 15, 20, 25, 30 };  // recommdation list

            UpdateX(userLinksTable);
            double loss = Loss(train, lambda);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                // UpdateX(userLinksTable, w);
                foreach (int uId in userItemsTable.Keys)
                {
                    if (userLinksTable.ContainsKey(uId))
                    {
                        List<Link> _links = (List<Link>)userLinksTable[uId];
                        UpdateX(uId, _links);
                    }

                    List<Rating> ratings = (List<Rating>)userItemsTable[uId]; // ratings with an UserId
                    foreach (Rating r in ratings)
                    {
                        double pui = Predict(r.UserId, r.ItemId);
                        double eui = r.Score - pui;
                        for (int i = 0; i < f; i++)
                        {
                            P[r.UserId, i] += gamma * (eui * (Q[r.ItemId, i] + X[r.UserId, i]) - lambda * P[r.UserId, i]);
                            Q[r.ItemId, i] += gamma * (eui * (P[r.UserId, i]) - lambda * Q[r.ItemId, i]);
                        }

                        // update W_u
                        double _sum = 0.0;
                        for (int i = 0; i < f; i++)
                        {
                            _sum += X[uId, i] * P[uId, i];
                        }
                        if (_sum != 0)
                        {
                            W[uId] += gamma * (eui * _sum - lambda * W[uId]);
                        }
                    }
                }

                double lastLoss = Loss(train, lambda);

                if (epoch % 2 == 0 && epoch >= 20)
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
    }
}
