using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// ICDM2011, p497, Ning
    /// SLIM: Sparse Linear Methods for Top-N Recommender Systems
    /// </summary>
    public class SLIM
    {
        public double[,] W { get; private set; }   // n * n


        public SLIM() { }
        public SLIM(int m, int n)
        {
            InitializeModel(m, n);
        }

        public void InitializeModel(int m, int n)
        {
            W = MathUtility.RandomGaussian(n, n, 0, 0.1);
            for (int i = 0; i < n; i++)
            {
                W[i, i] = 0.0;
            }


        }

    }
}
