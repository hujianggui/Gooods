using System;
using System.Collections.Generic;

using RS.CollaborativeFiltering;
using RS.DataType;
using RS.Data.Utility;

namespace RS.Data
{
    public static class ML_100k
    {
        public static string DefalultDirectory = @"D:\data\movielens\ml-100k\";

        public static string DefaultRatingFile = DefalultDirectory + @"u.data";
        public static string DefaultItemFile   = DefalultDirectory + @"u.item";
        public static string DefaultUserFile   = DefalultDirectory + @"u.user";

        public static string BaseRatingFile    = DefalultDirectory + @"u1.base";
        public static string TestRatingFile    = DefalultDirectory + @"u1.test";

        public static int MaxUserId = 943;
        public static int MaxItemId = 1682;


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

        public static void UserKNNTest(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile);
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            UserKNN knn = new UserKNN();
            knn.TryMaeRmse(data.Item1, data.Item2);
        }

        public static void MatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId, 10);
            model.TrySGD(baseRatings, testRatings, 100, 0.01, 0.01, 0.94);
        }

        public static void BiasedMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.01, 0.01, 0.95);
        }

        public static void SVDPlusPlusTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            SVDPlusPlus model = new SVDPlusPlus(MaxUserId, MaxItemId);            
            model.TrySGD(baseRatings, testRatings, 100, 0.01, 0.01, 0.96);
        }

        public static void AlternatingLeastSquaresTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            AlternatingLeastSquares model = new AlternatingLeastSquares(MaxUserId, MaxItemId, 10);
            model.TryALS(baseRatings, testRatings, 100, 0.08, 1, 5);
        }

        public static void EuclideanEmbeddingTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            EuclideanEmbedding model = new EuclideanEmbedding(MaxUserId, MaxItemId, 50);
            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.04, 1.0);
        }

        public static void UserKNNv2Test()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            UserKNNv2 knn = new UserKNNv2();
            //knn.TryTopN(data.Item1, data.Item2, 80, 10);
            knn.TryTopN(baseRatings, testRatings);
        }

        public static void UserKNNv2Test(double testSize = 0.1)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile);
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            UserKNNv2 knn = new UserKNNv2();
            //knn.TryTopN(data.Item1, data.Item2, 80, 10);
            knn.TryTopN(data.Item1, data.Item2);
        }

        public static void MatrixFactorizationTopNTest(double testSize = 0.125)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile);
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            var baseSamples = Tools.RandomSelectNegativeSamples(data.Item1, 2, true);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId, 100);
            model.TrySGDForTopN(baseSamples, data.Item2, 100, 0.02, 0.01, 0.9);
        }

        public static void AlternatingLeastSquaresTopNTest(double testSize = 0.125)
        {
            List<Rating> ratings = Tools.GetRatings(DefaultRatingFile);
            Tools.UpdateIndexesToZeroBased(ratings);
            var data = Tools.TrainTestSplit(ratings, testSize);
            var baseSamples = Tools.RandomSelectNegativeSamples(data.Item1, 2, true);

            AlternatingLeastSquares model = new AlternatingLeastSquares(MaxUserId, MaxItemId, 10);
            model.TryALSForTopN(baseSamples, data.Item2, 100, 0.01, 0, 1);
        }
    }
}
