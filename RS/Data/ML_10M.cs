using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.Algorithm;
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


        // Algorithm testing
        public static void MatrixFactorizationTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);

            var t = Tools.TrainTestSplit(ratings, testSize);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(t.Item1, t.Item2, 100);
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
