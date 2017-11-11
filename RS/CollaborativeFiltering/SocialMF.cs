using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.DataType;
using RS.Data.Utility;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// Recsys2010-p135-Jamali. 
    /// 2014.10.27, crate. 
    /// 2017.11.08, recreate.
    /// </summary>
    public class SocialMF : MatrixFactorization
    {
        protected double[,] X = null;   // user connections factors
        protected double[,] Y = null;   // user reverse connections factors

        public SocialMF() { }

        public override void InitializeModel(int p, int q, int f, string fillMethod = "uniform_df")
        {
            base.InitializeModel(p, q, f, fillMethod);
            X = new double[p, f];
            Y = new double[p, f];
        }

        public SocialMF(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            InitializeModel(p, q, f, fillMethod);
        }

        public double Loss(List<Rating> ratings, double lambda_U = 0.01, double lambda_V = 0.01, double lambda_T = 0.01)
        {
            double loss = 0.0;
            foreach(Rating r in ratings)
            {
                double eui = r.Score - Predict(r.UserId, r.ItemId);
                loss += eui * eui;

                double sum_p_i = 0.0;
                double sum_q_j = 0.0;
                double sum_regularizer = 0.0;
                for (int i = 0; i < f; i++)
                {
                    sum_p_i += P[r.UserId, i] * P[r.UserId, i];
                    sum_q_j += Q[r.ItemId, i] * Q[r.ItemId, i];
                    sum_regularizer += X[r.UserId, i] * X[r.UserId, i];
                }
                double regularizer = lambda_U * sum_p_i + lambda_V * sum_q_j + lambda_T * sum_regularizer;               
                loss += 0.5 * regularizer;               
            }
            return loss;
        }

        protected virtual void UpdateRegularizer(double[,] XorY, int userId, List<Link> links)
        {
            for (int i = 0; i < f; i++) // set to 0
            {
                XorY[userId, i] = 0;
            }

            foreach (Link t in links)
            {
                if (t.To > p)  // if linked user id not exist in dataset
                {
                    continue;
                }
                for (int i = 0; i < f; i++)
                {
                    XorY[userId, i] += P[t.To, i];
                }
            }
            for (int i = 0; i < f; i++)
            {
                XorY[userId, i] = P[userId, i] - XorY[userId, i] / links.Count;
            }
        }

        protected void PrintParameters(List<Rating> train, List<Rating> test, List<Link> links, int epochs = 100,
            double gamma = 0.01, double lambda_U = 0.01, double lambda_V = 0.01, double lambda_T = 0.01, double decay = 1,
            double minimumRating = 1.0, double maximumRating = 5.0)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("links,{0}", links.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("lambda_U,{0}", lambda_U);
            Console.WriteLine("lambda_V,{0}", lambda_V);
            Console.WriteLine("lambda_T,{0}", lambda_T);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("minimumRating,{0}", minimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }
        /// <summary>
        /// Update social regularizer term w.r.t. user connections regularization or 
        /// user reverse connections regularization.
        /// </summary>
        /// <param name="XorY">truster X, or trustee Y</param>
        /// <param name="userLinksTableOrUserReverseLinksTable"></param>
        protected void UpdateRegularizer(double[,] XorY, Hashtable userLinksTableOrUserReverseLinksTable)
        {
            foreach (int uId in userLinksTableOrUserReverseLinksTable.Keys)
            {
                List<Link> links = (List<Link>)userLinksTableOrUserReverseLinksTable[uId];
                UpdateRegularizer(XorY, uId, links);
            }
        }

        public void TrySGD(List<Rating> train, List<Rating> test, List<Link> links, int epochs = 100, 
            double gamma = 0.01, double lambda_U = 0.01, double lambda_V = 0.01, double lambda_T = 0.01, double decay = 1,
            double minimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, links, epochs, gamma, 
                lambda_U,lambda_V, lambda_T, decay, 
                minimumRating, maximumRating);
            Console.WriteLine("epoch,train:loss,test:mae,test:rmse");

            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable userLinksTable   = Tools.GetUserLinksTable(links); 
            Hashtable userReverseLinksTable = Tools.GetUserReverseLinksTable(links);
            UpdateRegularizer(X, userLinksTable);
            UpdateRegularizer(Y, userReverseLinksTable);
            double loss = Loss(train, lambda_U, lambda_V, lambda_T);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach (int userId in userItemsTable.Keys)
                {
                    if (userLinksTable.ContainsKey(userId))
                    {
                        List<Link> _links = (List<Link>)userLinksTable[userId];
                        UpdateRegularizer(X, userId, _links);
                    }
                    if (userReverseLinksTable.ContainsKey(userId))
                    {
                        List<Link> _links = (List<Link>)userReverseLinksTable[userId];
                        UpdateRegularizer(Y, userId, _links);
                    }

                    List<Rating> ratings = (List<Rating>)userItemsTable[userId]; 
                    foreach (Rating r in ratings)
                    {
                        double pui = Predict(r.UserId, r.ItemId);
                        double eui = r.Score - pui;
                        for (int i = 0; i < f; i++)
                        {
                            P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda_U * P[r.UserId, i] + lambda_T * (X[r.UserId, i] + Y[r.UserId, i]));
                            Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda_V * Q[r.ItemId, i]);
                        }
                    }
                }

                double lastLoss = Loss(train, lambda_U, lambda_V, lambda_T);
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
