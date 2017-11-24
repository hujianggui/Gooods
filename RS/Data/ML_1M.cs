
using System;
using System.Collections.Generic;

using RS.CollaborativeFiltering;
using RS.DataType;
using RS.Data.Utility;

namespace RS.Data
{
    public class ML_1M
    {
        public static string DefalultDirectory = @"D:\data\movielens\ml-1m\";

        public static string DefaultRatingFile = DefalultDirectory + @"ratings.dat";
        public static string DefaultItemFile = DefalultDirectory + @"movies.dat";
        public static string DefaultUserFile = DefalultDirectory + @"users.dat";

        public static string BaseRatingFile = DefalultDirectory + @"u1.base";
        public static string TestRatingFile = DefalultDirectory + @"u1.test";

        public static int MaxUserId = 6040;
        public static int MaxItemId = 3952;

        public static void UpdateDataInformation()
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            var q = Tools.GetMaxUserIdAndItemId(ratings);
            MaxUserId = q.Item1;
            MaxItemId = q.Item2;
        }

        public static void MeanFillingTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, "::");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, "::");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MeanFilling.TryGlobalMean(baseRatings, testRatings, true);
            MeanFilling.TryUserMean(baseRatings, testRatings, true);
            MeanFilling.TryItemMean(baseRatings, testRatings, true);
        }

        public static void UserKNNTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            UserKNN knn = new UserKNN();
            knn.TryMaeRmse(data.Item1, data.Item2);
        }

        public static void Preprocess()
        {
            List<Rating> ratings = Tools.GetRatings(DefalultDirectory + "ratings.dat", "::");
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, 0.2);
        }

        public static void UserKNNv2Test(double testSize = 0.125)
        {
            List<Rating> ratings = Tools.GetRatings(DefalultDirectory + "ratings.dat", "::");
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            UserKNNv2 knn = new UserKNNv2();
            //knn.TryTopN(data.Item1, data.Item2, 80, 10);
            knn.TryTopN(data.Item1, data.Item2);
        }

        public static void ItemKNNv2Test(double testSize = 0.125)
        {
            List<Rating> ratings = Tools.GetRatings(DefalultDirectory + "ratings.dat", "::");
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            ItemKNNv2 knn = new ItemKNNv2();
            //knn.TryTopN(data.Item1, data.Item2, 10, 10);
            knn.TryTopN(data.Item1, data.Item2);
        }

        public static void MatrixFactorizationTopNTest(double testSize = 0.125)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            var baseSamples = Tools.RandomSelectNegativeSamples(data.Item1, 1, true);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId, 100);
            model.TrySGDForTopN(baseSamples, data.Item2, 100, 0.02, 0.01, 0.9);
        }

        // 2017.10.26
        public static void BiasedMatrixFactorizationTopNTest(double testSize = 0.125)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            var baseSamples = Tools.RandomSelectNegativeSamples(data.Item1, 1, true);

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId, 100);
            model.TrySGDForTopN(baseSamples, data.Item2, 100, 0.02, 0.01, 0.9);
        }

        public static void MatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, "::");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, "::");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.01, 0.01);
        }


        public static void BiasedMatrixFactorizationTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);

            var t = Tools.TrainTestSplit(ratings, testSize);

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(t.Item1, t.Item2, 100);
        }

        public static void SVDPlusPlusTest(double testSize=0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile, "::");
            Tools.UpdateIndexesToZeroBased(ratings);

            var t = Tools.TrainTestSplit(ratings, testSize);

            SVDPlusPlus model = new SVDPlusPlus(MaxUserId, MaxItemId);
            model.TrySGD(t.Item1, t.Item2, 100);
        }

    }
}
