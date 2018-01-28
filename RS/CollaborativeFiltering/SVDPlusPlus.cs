using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using RS.DataType;
using RS.Core;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// 
    /// </summary>
    public class SVDPlusPlus
    {
        protected int p = 0;   // Number of Users
        protected int q = 0;   // Number of Items
        protected int f = 10;  // Number of features
        public double w = 1.0;  // weight of neighbors

        public double[,] P { get; protected set; }  // Matrix consists of user features
        public double[,] Q { get; protected set; }   // Matrix consists of item features
        public double[,] Z { get; protected set; }   // sum of Yj which j belongs to N(u), N(u) presents items rated by user u
        public double[,] Y { get; protected set; }

        public double[] bu = null;  // bias of user u
        public double[] bi = null;  // bias of item i

        public SVDPlusPlus() { }

        public SVDPlusPlus(int p, int q, int f = 10)
        {
            InitializeModel(p, q, f);
        }

        public void InitializeModel(int p, int q, int f, string fillMethod = "uniform_f")
        {
            this.p = p;
            this.q = q;
            this.f = f;

            bu = new double[this.p];
            bi = new double[this.q];

            if (fillMethod == "uniform_f")
            {
                P = MathUtility.RandomUniform(p, f, 1.0 / Math.Sqrt(f));
                Q = MathUtility.RandomUniform(q, f, 1.0 / Math.Sqrt(f));
                Z = MathUtility.RandomUniform(p, f, 1.0 / Math.Sqrt(f));
                Y = MathUtility.RandomUniform(q, f, 1.0 / Math.Sqrt(f));

            }
            else if (fillMethod == "gaussian")
            {
                P = MathUtility.RandomGaussian(p, f, 0, 1);
                Q = MathUtility.RandomGaussian(q, f, 0, 1);
                Z = MathUtility.RandomGaussian(p, f, 0, 1);
                Y = MathUtility.RandomGaussian(q, f, 0, 1);
            }
            else if (fillMethod == "uniform")
            {
                P = MathUtility.RandomUniform(p, f);
                Q = MathUtility.RandomUniform(q, f);
                Z = MathUtility.RandomUniform(p, f);
                Y = MathUtility.RandomUniform(q, f);
            }

        }

        public virtual double Predict(int userId, int itemId, double miu)
        {
            double _r = 0.0;
            for (int i = 0; i < this.f; i++)
            {
                _r += Q[itemId, i] * (P[userId, i] + Z[userId, i]);
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
                double sum_y_j = 0.0;

                for (int i = 0; i < this.f; i++)
                {
                    sum_p_i += P[r.UserId, i] * P[r.UserId, i];
                    sum_q_j += Q[r.ItemId, i] * Q[r.ItemId, i];
                    sum_y_j += Y[r.ItemId, i] * Y[r.ItemId, i];
                }
                loss += lambda * 0.5 * (sum_p_i + sum_q_j + sum_y_j + bu[r.UserId] * bu[r.UserId] + bi[r.ItemId] * bi[r.ItemId]);
            }
            return loss;
        }

        public void EvaluateMaeRmse(List<Rating> ratings, double lambda, double miu, out double mae, out double rmse)
        {
            mae = rmse = 0;

            foreach (Rating r in ratings)
            {
                double pui = Predict(r.UserId, r.ItemId, miu);
                double eui = r.Score - pui;

                mae += Math.Abs(eui);
                rmse += eui * eui;
            }

            if (ratings.Count > 0)
            {
                mae /= ratings.Count;
                rmse = Math.Sqrt(rmse / ratings.Count);
            }
        }

        protected Hashtable GetUserItemsTable(List<Rating> ratings)
        {
            Hashtable userItemsTable = new Hashtable();
            foreach (Rating r in ratings)
            {
                if (userItemsTable.ContainsKey(r.UserId))
                {
                    List<Rating> li = (List<Rating>)userItemsTable[r.UserId];
                    li.Add(r);
                }
                else
                {
                    userItemsTable.Add(r.UserId, new List<Rating>() { r });
                }
            }
            return userItemsTable;
        }

        protected void UpdataZ(Hashtable userItemsTable) // Z = sum(yj), j belongs to N(u), N(u) presents items rated by u
        {
            foreach (int uId in userItemsTable.Keys)
            {
                List<Rating> list = (List<Rating>)userItemsTable[uId];
                foreach (Rating r in list)
                {
                    for (int i = 0; i < f; i++)
                    {
                        Z[uId, i] += Y[r.ItemId, i];
                    }
                }
                if (list.Count > 1)
                {
                    for (int i = 0; i < f; i++)
                    {
                        Z[uId, i] /= Math.Sqrt(list.Count);
                    }
                }
            }
        }

        protected void UpdataZ(int uId, List<Rating> ratings, double ru) // Z = sum(yj), j belongs to N(u), N(u) presents items rated by u
        {
            foreach (Rating r in ratings)
            {
                for (int i = 0; i < f; i++)
                {
                    Z[uId, i] += Y[r.ItemId, i];
                }
            }

            for (int i = 0; i < f; i++)
            {
                Z[uId, i] *= ru;
            }
        }

        protected void PrintParameters(List<Rating> train, List<Rating> test = null, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double minimumRating = 1.0, double maximumRating = 5.0)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test == null ? 0 : test.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("minimumRating,{0}", minimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        public void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double minimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, epochs, gamma, lambda, decay, minimumRating, maximumRating);
            Console.WriteLine("epoch,train:loss,test:mae,test:rmse");
            double miu = train.Average(r => r.Score);
            Hashtable userItemsTable = GetUserItemsTable(train);
            UpdataZ(userItemsTable);

            double loss = Loss(train, lambda, miu);

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                foreach (int uId in userItemsTable.Keys)
                {
                    List<Rating> li = (List<Rating>)userItemsTable[uId];
                    double ru = w / Math.Sqrt(li.Count);
                    UpdataZ(uId, li, ru);   // NOTE: different from the provided in Java, posite here to reduce complexity.
                    double[] sum = new double[f];

                    foreach (Rating r in li)
                    {
                        double pui = Predict(r.UserId, r.ItemId, miu);
                        double eui = r.Score - pui;
                        bu[r.UserId] += gamma * (eui - lambda * bu[r.UserId]);
                        bi[r.ItemId] += gamma * (eui - lambda * bi[r.ItemId]);

                        for (int i = 0; i < f; i++)
                        {
                            sum[i] += Q[r.ItemId, i] * eui * ru;
                            P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] - lambda * P[r.UserId, i]);
                            Q[r.ItemId, i] += gamma * (eui * (P[r.UserId, i] + Z[r.UserId, i]) - lambda * Q[r.ItemId, i]);
                        }
                    }

                    foreach (Rating r in li)
                    {
                        for (int i = 0; i < f; i++)
                        {
                            Y[r.ItemId, i] += gamma * (sum[i] - lambda * Y[r.ItemId, i]);
                        }
                    }
                }

                double lastLoss = Loss(train, lambda, miu);
                double mae = 0.0, rmse = 0.0;
                EvaluateMaeRmse(test, lambda, miu, out mae, out rmse);
                Console.WriteLine("{0},{1},{2},{3}", epoch, lastLoss, mae, rmse);

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
