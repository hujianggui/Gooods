using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;
using System.Collections;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// ICDM2011, p497, Ning
    /// SLIM: Sparse Linear Methods for Top-N Recommender Systems
    /// 1. 全量 2. kNN
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

        public double Predict(int userId, int itemId, int[] neighbors, int excludeItemId)
        {
            double sum = 0.0; // A ranking score
            foreach(int n in neighbors)
            {
                if (n != excludeItemId)
                {
                    sum += W[itemId, n];
                }
            }
            return 0;
        }


        public void TryLeastSquares(List<Rating> train, List<Rating> test, int maxItemId, double beta, double lambda)
        {
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable itemUsersTable = Tools.GetItemUsersTable(train);

            ItemKNNv2 itemkNN = new ItemKNNv2();
            MyTable coourrenceTable = itemkNN.CalculateCooccurrences(userItemsTable, true);
            MyTable wuv = itemkNN.CalculateSimilarities(coourrenceTable, itemUsersTable);
            Hashtable similarItems = itemkNN.GetSimilarItems(wuv, 80);

            foreach (int itemId in similarItems.Keys)
            {
                List<Link> list = (List<Link>)similarItems[itemId];
                Console.Write("{0}", itemId);
                foreach (Link l in list)
                {
                    Console.WriteLine(",{0},{1}", l.To, l.Weight);
                }
            }

            for (int itemId = 0; itemId <= maxItemId; itemId++)
            {

            }
        }

    }
}
