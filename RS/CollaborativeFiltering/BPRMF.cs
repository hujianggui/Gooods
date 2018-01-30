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
    /// UAI2009, p452, Stefen Rendle, BPR: Bayesian Personalized Ranking from Implicit Feedback
    /// </summary>
    public class BPRMF : MatrixFactorization
    {
        public BPRMF() { }

        public BPRMF(int p, int q, int f = 10, string fillMethod = "uniform_df")
        {
            base.InitializeModel(p, q, f, fillMethod);
        }

        protected Tuple<int, int, int>  SampleItemPair(int userId, Hashtable itemsTable, int[] itemIds, Random random)
        {
            int itemId = itemIds[random.Next(itemIds.Length)];
            while (!itemsTable.ContainsKey(itemId))
            {
                itemId = itemIds[random.Next(itemIds.Length)];
            }

            int otherItemId = itemIds[random.Next(itemIds.Length)];
            while (itemsTable.ContainsKey(otherItemId))
            {
                otherItemId = itemIds[random.Next(itemIds.Length)];
            }

            return Tuple.Create(userId, itemId, otherItemId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ratingTable"></param>
        /// <param name="ratio"></param>
        /// <returns>list of triples: user id - item id - other item id</returns>
        protected List<Tuple<int, int, int>> SampleTriples(MyTable ratingTable, int ratio = 100)
        {
            var userIds = ratingTable.GetMainKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            var itemIds = ratingTable.GetSubKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            List<Tuple<int, int, int>> list = new List<Tuple<int, int, int>>();

            var random = Core.Random.GetInstance();
            for (int i = 0; i < userIds.Length * 100; i++)
            {
                // randomly select a user
                int userId = userIds[random.Next(userIds.Length)];
                Hashtable itemsTable = (Hashtable)ratingTable[userId];

                var triples = SampleItemPair(userId, itemsTable, itemIds, random);
                list.Add(triples);
            }
            return list;
        }

        public void TryTopN(List<Rating> train, List<Rating> test, int epochs = 100)
        {
            var ratingTable = Tools.GetRatingTable(train);
            var triples = SampleTriples(ratingTable, 100);

            for (int e = 1; e <= epochs; e++)
            {
                foreach (var t in triples)
                {
                    
                }
            }
        }
    }
}
