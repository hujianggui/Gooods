using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.CollaborativeFiltering;
using RS.DataType;
using RS.Data.Utility;

namespace RS.Data
{
    public class ML_10M
    {
        public static string DefalultDirectory = @"D:\data\movielens\ml-10M100K\";

        public static string DefaultRatingFile = DefalultDirectory + @"ratings.dat";
        public static string DefaultItemFile = DefalultDirectory + @"movies.dat";
        public static string DefaultUserFile = DefalultDirectory + @"users.dat";

        public static string BaseRatingFile = DefalultDirectory + @"u1.base";
        public static string TestRatingFile = DefalultDirectory + @"u1.test";

        public static int MaxUserId = 71567;
        public static int MaxItemId = 65133;


        // Preprocess
        public static void UpdateDataInformation()
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            var q = Tools.GetMaxUserIdAndItemId(ratings);
            MaxUserId = q.Item1;
            MaxItemId = q.Item2;
            Console.WriteLine(q);
        }

        /// <summary>
        /// CDAE, WSDM2016-p153-Wu
        /// Collaborative Denoising Auto-Encoders for Top-N Recommender Systems (Top-N recommendation)
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static List<Rating> FilterByScore(List<Rating> ratings)
        {
            var query = from r in ratings
                        where r.Score >= 4
                        select r;

            var q1 = query.Select(r => r.UserId).Distinct().Count();
            var q2 = query.Select(r => r.ItemId).Distinct().Count();

            Console.WriteLine(q1);
            Console.WriteLine(q2);
            Console.WriteLine(query.Count());

            return query.ToList();
        }

        public static void Split()
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            var data = Tools.TrainTestSplit(ratings, 0.2);
            Tools.WriteTimedRatings(data.Item1, BaseRatingFile, "\t");
            Tools.WriteTimedRatings(data.Item2, TestRatingFile, "\t");
        }

        public static void MeanFillingTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MeanFilling.TryGlobalMean(baseRatings, testRatings, true);
            MeanFilling.TryUserMean(baseRatings, testRatings, true);
            MeanFilling.TryItemMean(baseRatings, testRatings, true);
        }


        // Algorithm testing
        public static void MatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100);
        }

        public static void MatrixFactorizationTest(double testSize = 0.2)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);

            var t = Tools.TrainTestSplit(ratings, testSize);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(t.Item1, t.Item2, 100, 0.005);
        }



        public static void BiasedMatrixFactorizationTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);

            var t = Tools.TrainTestSplit(ratings, testSize);

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(t.Item1, t.Item2, 100, 0.005);
        }
    }
}
