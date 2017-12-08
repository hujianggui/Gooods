using RS.DataType;
using System;
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

        public double[,] P { get; protected set; }  // Matrix consists of user features
        public double[,] Q { get; protected set; }  // Matrix consists of item features

        public double[] bu { get; protected set; }  // user biases
        public double[] bi { get; protected set; }  // item biases

        public double miu { get; protected set; }   // mean rating of train data

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
        }


        public virtual double Predict(int userId, int itemId)
        {
            double _r = 0.0;
            for (int i = 0; i < f; i++)
            {
                _r += P[userId, i] * Q[itemId, i];
            }
            return _r + miu + bu[userId] + bi[itemId];
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
                loss += lambda * 0.5 * (sum_p_i + sum_q_j + bu[r.UserId] * bu[r.UserId] + bi[r.ItemId] * bi[r.ItemId]);
            }
            return loss;
        }


        public Tuple<double, double> EvaluateMaeRmse(List<Rating> ratings, double minimumRating = 1.0, double maximumRating = 5.0)
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

        public void PrintParameters(List<Rating> train, List<Rating> test = null, int epochs = 100, 
            double gamma = 0.01, double lambda = 0.01, double decay = 1.0, 
            double minimumRating = 1.0, double maximumRating = 5.0)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("p,{0},q,{1},f,{2}", p, q, f);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("gamma,{0}", gamma);
            Console.WriteLine("lambda,{0}", lambda);
            Console.WriteLine("decay,{0}", decay);
            Console.WriteLine("minimumRating,{0}", minimumRating);
            Console.WriteLine("maximumRating,{0}", maximumRating);
        }

        public void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, 
            double gamma = 0.01, double lambda = 0.01, double decay = 1.0)
        {
            double minimumRating = train.AsParallel().Min(r => r.Score);
            double maximumRating = train.AsParallel().Max(r => r.Score);

            PrintParameters(train, test, epochs, gamma, lambda, decay, minimumRating, maximumRating);
            Console.WriteLine("epoch,loss,test:mae,test:rmse");

            double miu = train.AsParallel().Average(r => r.Score);
            double loss = Loss(train, lambda);


        }



    }
}
