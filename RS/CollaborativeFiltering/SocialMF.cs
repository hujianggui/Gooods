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

        public void TrySGD(List<Rating> train, List<Rating> test, List<Link> links, int epochs = 100, 
            double gamma = 0.01, double lambda_U = 0.01, double lambda_V = 0.01, double lambda_T = 0.01, double decay = 1,
            double minimumRating = 1.0, double maximumRating = 5.0)
        {
            Hashtable userRatingsTable = Tools.GetUserItemsTable(train);
            Hashtable userLinksTable   = Tools.GetUserLinksTable(links);

            



        }


    }
}
