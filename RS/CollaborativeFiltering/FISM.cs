using RS.Data.Utility;
using RS.DataType;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// KDD2013-p659-Kabbur
    /// FISM: Factored Item Similarity Models for Top-N Recommender Systems
    /// </summary>
    public class FISM
    {
        protected int p = 0;   // Number of Users
        protected int q = 0;   // Number of Items
        protected int f = 10;  // Number of features

        public double[,] P { get; protected set; }  // Matrix consists of item features, left side
        public double[,] Q { get; protected set; }  // Matrix consists of item features, right side

        public double[] bu { get; protected set; }  // user biases
        public double[] bi { get; protected set; }  // item biases


        protected double[,] X { get; set; }   //  average item neighbors


        public FISM() { }

        public FISM(int p, int q, int f = 10)
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

            P = MathUtility.RandomUniform(p, f, -0.001, 0.001);
            Q = MathUtility.RandomUniform(q, f, -0.001, 0.001);
            X = new double[p, f];
        }

        /// <summary>
        /// The pdf says: $$\hat(r_{ui}) = b_u + b_i + q_i^T x$$
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="itemId"></param>
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

        protected void UpdateX(int userId, List<Rating> neighbors, int excludeItemId, double factor)
        {
            foreach (Rating r in neighbors)
            {
                if (r.ItemId != excludeItemId)
                {
                    for (int i = 0; i < f; i++)
                    {
                        X[userId, i] += Q[r.ItemId, i];
                    }
                }
            }
            for (int i = 0; i < f; i++)
            {
                X[userId, i] *= factor;
            }
        }

        /// <summary>
        /// Equ. (8)
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        protected virtual double Loss(List<Rating> ratings, double lambda_P, double lambda_Q, double lambda_bu, double lambda_bi)
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
                sum_p_i *= lambda_P;
                sum_q_j *= lambda_Q;
                loss += 0.5 * (sum_p_i + sum_q_j + lambda_bu * bu[r.UserId] * bu[r.UserId] + lambda_bi * bi[r.ItemId] * bi[r.ItemId]);
            }
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

        //protected void PrintParameters(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double decay = 1.0,
        //    double aplah = 1, double lambda_P = 0.01, double lambda_Q = 0.01, double lambda_bu = 0.01, double lambda_bi = 0.01, 
        //    double minimumRating = 1.0, double maximumRating = 5.0)
        //{
        //    Console.WriteLine(GetType().Name);
        //    Console.WriteLine("train,{0}", train.Count);
        //    Console.WriteLine("test,{0}", test.Count);
        //    Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
        //    Console.WriteLine("epochs,{0}", epochs);
        //    Console.WriteLine("gamma,{0}", gamma);
        //    Console.WriteLine("decay,{0}", decay);
        //    Console.WriteLine("aplah,{0}", aplah);
        //    Console.WriteLine("lambda_P,{0}", lambda_P);
        //    Console.WriteLine("lambda_Q,{0}", lambda_Q);
        //    Console.WriteLine("lambda_bu,{0}", lambda_bu);
        //    Console.WriteLine("lambda_bi,{0}", lambda_bi);
        //    Console.WriteLine("minimumRating,{0}", minimumRating);
        //    Console.WriteLine("maximumRating,{0}", maximumRating);
        //}



        /// <summary>
        /// 
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="epochs"></param>
        /// <param name="gamma"></param>
        /// <param name="decay"></param>
        /// <param name="alpha"></param>
        /// <param name="lambda_P"></param>
        /// <param name="lambda_Q"></param>
        /// <param name="lambda_bu"></param>
        /// <param name="lambda_bi"></param>
        public void TrySGDForRMSE(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double decay = 1.0, 
            double alpha = 1, double lambda_P = 0.01, double lambda_Q = 0.01, double lambda_bias = 0.01)
        {
            double minimumRating = train.AsParallel().Min(r => r.Score);
            double maximumRating = train.AsParallel().Max(r => r.Score);

            //PrintParameters(train, test, epochs, gamma, decay, alpha,
            //    lambda_P, lambda_Q, lambda_bu, lambda_bi, 
            //    minimumRating, maximumRating);

            Console.WriteLine("epoch,loss,test:mae,test:rmse");
            double miu = train.AsParallel().Average(r => r.Score);
            double loss = Loss(train, lambda_P, lambda_Q, lambda_bu, lambda_bi);

            int rho = 3;

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                var ratings = Tools.RandomSelectNegativeSamples(train, rho, false); //Tools.SampleZeros(train, rho, false);


                Hashtable userItemsTable = Tools.GetUserItemsTable(ratings);

                foreach (int uId in userItemsTable.Keys)
                {
                    List<Rating> li = (List<Rating>)userItemsTable[uId];
                    double factor = Math.Pow(li.Count - 1, -alpha);

                    foreach (Rating r in li)
                    {
                        UpdateX(r.UserId, li, r.ItemId, factor);
                        double pui = Predict(r.UserId, r.ItemId);
                        double eui = r.Score - pui;
                        bu[r.UserId] += gamma * (eui - lambda_bu * bu[r.UserId]);
                        bi[r.ItemId] += gamma * (eui - lambda_bi * bi[r.ItemId]);

                        for (int i = 0; i < f; i++)
                        {
                            P[r.UserId, i] += gamma * (eui * Q[r.ItemId, i] * factor - lambda_P * P[r.UserId, i]);
                            Q[r.ItemId, i] += gamma * (eui * P[r.UserId, i] - lambda_Q * Q[r.ItemId, i]);
                        }
                    }
                }

                double lastLoss = Loss(train, lambda_P, lambda_Q, lambda_bu, lambda_bi);
                var evaluate = EvaluateMaeRmse(test, minimumRating, maximumRating);
                Console.WriteLine("{0},{1},{2},{3}", epoch, lastLoss, evaluate.Item1, evaluate.Item2);

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
