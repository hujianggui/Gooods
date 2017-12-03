using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.DataType;
using RS.Data.Utility;


namespace RS.Evaluation
{
    public static class Metrics
    {
        /// <summary>
        /// Precision and Recall metrics which are used for top-k in recommender system.
        /// </summary>
        /// <param name="recommended"></param>
        /// <param name="test"></param>
        /// <returns>precision, recall</returns>
        public static Tuple<double, double> PrecisionAndRecall(List<Rating> recommended, List<Rating> test)
        {
            MyTable recommendedTable = new MyTable();
            foreach(Rating r in recommended)
            {
                if (!recommendedTable.ContainsKey(r.UserId, r.ItemId))
                {
                    recommendedTable.Add(r.UserId, r.ItemId, r.Score);
                }
            }

            int hit = 0;
            foreach (Rating r in test)
            {
                if (recommendedTable.ContainsKey(r.UserId, r.ItemId))
                {
                    hit++;
                }
            }

            double precision = 0.0;
            double recall = 0.0;
            if (recommended.Count > 0)
            {
                precision = hit * 1.0 / recommended.Count;
            }
            if (test.Count > 0)
            {
                recall = hit * 1.0 / test.Count;
            }
            return Tuple.Create(precision, recall);
        }

        /// <summary>
        /// Average popularity
        /// TODO: need to be further considered. 2017.06.09
        /// </summary>
        /// <param name="recommended"></param>
        /// <param name="train"></param>
        /// <returns></returns>
        public static Tuple<double, double> CoverageAndPopularity(List<Rating> recommended, List<Rating> train)
        {
            Hashtable trainTable = Tools.GetItemUsersTable(train);
            Hashtable recommendedTable = Tools.GetItemUsersTable(recommended);

            double coverage = 0.0;
            if (trainTable.Keys.Count > 0)
            {
                coverage = recommendedTable.Keys.Count * 1.0 / trainTable.Keys.Count;
            }

            double popularity = 0.0;        // TODO: need to be further considered. 2017.06.09
            foreach (Rating r in recommended)
            {
                List<Rating> li = (List<Rating>)trainTable[r.ItemId];
                popularity += Math.Log(1 + li.Count);
            }
            popularity /= recommended.Count;
            return Tuple.Create(coverage, popularity);
        }

        /// <summary>
        /// Mean average precision
        /// https://www.kaggle.com/c/coupon-purchase-prediction#evaluation
        /// </summary>
        /// <param name="recommended">sorted predicted ratings</param>
        /// <param name="test">real ratings</param>
        /// <returns></returns>
        public static double MAP(List<Rating> recommendations, List<Rating> test, int k = 5)
        {
            Hashtable recommendedRatings = Tools.GetUserItemsTable(recommendations);
            MyTable testTable = Tools.GetRatingTable(test);    // mark
            int validateUserCounter = 0;
            double _MAP = 0.0;

            foreach (int userId in recommendedRatings.Keys)
            {
                if (testTable.ContainsMainKey(userId))
                {
                    List<Rating> recommendedUserRatings = (List<Rating>)recommendedRatings[userId];
                    Hashtable testUserRatings = (Hashtable)testTable[userId];

                    int length = (k > recommendedUserRatings.Count ? recommendedUserRatings.Count : k);
                    int[] accuracy = new int[length];
                    int correctlyPredictedItems = 0;
                    for (int i = 0; i < length; i++)
                    {
                        if (testUserRatings.ContainsKey(recommendedUserRatings[i].ItemId))  // correctly predicted
                        {
                            correctlyPredictedItems++;
                            accuracy[i] = correctlyPredictedItems;
                        }
                    }

                    if (correctlyPredictedItems > 0)
                    {
                        double APu = 0.0;   // average precision of user u
                        for (int i = 0; i < length; i++)
                        {
                            APu += (accuracy[i] * 1.0 / (i + 1));
                        }
                        APu /= length;
                        _MAP += APu;
                    }
                    validateUserCounter++;  // recommended users which also in test 
                }
            }

            if (validateUserCounter > 0)
            {
                return _MAP / validateUserCounter;
            }

            return 0.0;
        }


        /// <summary>
        /// The test functin for the MAP metric, the example in:
        /// http://fastml.com/what-you-wanted-to-know-about-mean-average-precision/
        /// </summary>
        public static void MAPTest()
        {
            List<Rating> test = new List<Rating>(){ new Rating(1, 1, 1),
                                                    new Rating(1, 2, 1),
                                                    new Rating(1, 3, 1),
                                                    new Rating(1, 4, 1),
                                                    new Rating(1, 5, 1)};

            List<Rating> recommendations = new List<Rating>() { new Rating(1, 6, 1),
                                                                new Rating(1, 4, 1),
                                                                new Rating(1, 7, 1),
                                                                new Rating(1, 1, 1),
                                                                new Rating(1, 2, 1)};

            for (int i = 1; i <= 5;i++)
            {
                double _MAP = MAP(recommendations, test, i);
                Console.WriteLine("MAP@{0},{1}", i, _MAP);
            }

        }


        /// <summary>
        /// normalized discounted cumulative gain 
        /// </summary>
        /// <param name="recommended">predicted ratings</param>
        /// <param name="test">real ratings</param>
        /// <returns></returns>
        public static double nDCG(List<Rating> recommended, List<Rating> test)
        {
            return 1.0;
        }
    }

}
