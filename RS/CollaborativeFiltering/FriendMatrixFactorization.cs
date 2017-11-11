using System;
using System.Collections.Generic;
using System.Collections;

using RS.Data.Utility;
using RS.DataType;


namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// public in PAKDD2015
    /// </summary>
    public class FriendMatrixFactorization : MatrixFactorization
    {
        protected double[,] X = null;   // weighted linked factors

        public FriendMatrixFactorization() { }

        public override void InitializeModel(int p, int q, int f, string fillMethod = "uniform_df")
        {
            base.InitializeModel(p, q, f, fillMethod);
            X = new double[p, f];
        }

        public FriendMatrixFactorization(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            InitializeModel(p, q, f, fillMethod);
        }

        protected virtual void UpdateX(int uId, List<Link> links, double w)
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
                X[uId, i] *= (w * 1.0 / links.Count);   //  (w / Math.Sqrt(friends.Count));
            }
        }

        protected void UpdateX(Hashtable userLinksTable, double w)
        {
            foreach (int uId in userLinksTable.Keys)
            {
                List<Link> links = (List<Link>)userLinksTable[uId];
                UpdateX(uId, links, w);
            }
        }

        public override double Predict(int uId, int iId)
        {
            double _r = 0.0;
            for (int i = 0; i < f; i++)
            {
                // _r += Q[iId, i] * (P[uId, i] +  X[uId, i]);  // 修正用户P，xi为朋友们的隐式特征
                _r += P[uId, i] * (Q[iId, i] + X[uId, i]);    // 修正物品Q，xi为朋友们的隐式特征
            }
            return _r;
        }

        private void PrintParameters(List<Rating> train, List<Rating> test, List<Link> links, double w = 1.0, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double minimumRating = 1.0, double maximumRating = 5.0)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test == null ? 0 : test.Count);
            Console.WriteLine("links,{0}", links.Count);
            Console.WriteLine("w,{0}", w);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("minimumRating,{0}", minimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        public void TrySGD(List<Rating> train, List<Rating> test, List<Link> links, double w = 1.0, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double minimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, links, w, epochs, gamma, lambda, decay, minimumRating, maximumRating);
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable userLinksTable = Tools.GetUserLinksTable(links);

            Console.WriteLine("epoch,loss,test:mae,test:rmse");
            UpdateX(userLinksTable, w);
            double loss = Loss(train, lambda);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                // UpdateX(userLinksTable, w);
                foreach (int uId in userItemsTable.Keys)
                {                    
                    if (userLinksTable.ContainsKey(uId))
                    {
                        List<Link> _links = (List<Link>)userLinksTable[uId];
                        UpdateX(uId, _links, w);
                    }
                      
                    List<Rating> ratings = (List<Rating>)userItemsTable[uId]; // ratings with an UserId
                    foreach (Rating r in ratings)
                    {
                        double pui = Predict(r.UserId, r.ItemId);
                        double eui = r.Score - pui;
                        for (int i = 0; i < f; i++)
                        {
                            P[r.UserId, i] += gamma * (eui * (Q[r.ItemId, i] + X[r.UserId, i]) - lambda * P[r.UserId, i]);
                            Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda * Q[r.ItemId, i]);
                        }
                    }
                }

                double lastLoss = Loss(train, lambda);
                var eval = EvaluateMaeRmse(test, minimumRating, maximumRating);
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
    }
}
