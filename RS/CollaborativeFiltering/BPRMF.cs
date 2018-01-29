﻿using RS.Data.Utility;
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


        protected List<Tuple<int, int, int>> SampleTriples(MyTable ratingTable)
        {
            var userIds = ratingTable.GetMainKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();
            var itemIds = ratingTable.GetSubKeyArray().AsParallel().Cast<int>().OrderBy(k => k).ToArray();

            var random = Core.Random.GetInstance();

            // randomly select a user

            int userId = userIds[random.Next(userIds.Length)];
            Hashtable itemsTable = (Hashtable)ratingTable[userId];

            // randomly select an item

            int i = itemIds[random.Next(itemIds.Length)];

            List<Tuple<int, int, int>> list = new List<Tuple<int, int, int>>();


            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ratingTable">user id - item id - score table</param>
        /// <returns></returns>
        protected Tuple<int, int, int> SampleTriple(MyTable ratingTable)
        {

            return Tuple.Create(1, 1, 1);
        }


        public void TryTopN(List<Rating> train, int epochs = 100)
        {
            var ratingTable = Tools.GetRatingTable(train);
            var triples = SampleTriples(ratingTable);

        }

    }
}
