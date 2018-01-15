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

        public static void Preprocess()
        {
            string file = DefalultDirectory + "u1.pred.K80N30.txt";
            var pred = Tools.GetRatings(file, " ");
            var test = Tools.GetRatings(TestRatingFile);

            int[] Ns = { 1, 5, 10, 15, 20, 25, 30};
            foreach(int n in Ns)
            {
                var recommend = Tools.GetSubset(pred, n);
                var pr = Evaluation.Metrics.PrecisionAndRecall(recommend, test);
                Console.WriteLine("{0},{1},{2}", n, pr.Item1, pr.Item2);
            }
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
            //knn.TryTopN(baseRatings, testRatings, 80, 10);
            knn.TryTopN(baseRatings, testRatings);
        }

        public static void ItemKNNv2Test()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            ItemKNNv2 knn = new ItemKNNv2();
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

        public static void PageRankTopNTest()
        {
            // PageRank algirthm could be converged in top-N recommendation.
            List<Link> baseLinks = Tools.GetLinks(BaseRatingFile);
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseLinks);
            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            var maxIds = Tools.GetMaxNodeId(baseLinks);
            int nodes = Tools.TransformLinkedToId(baseLinks, maxIds.Item1, maxIds.Item2);

            PageRank pr = new PageRank(nodes);

            double[] steps = { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.8, 0.9 };            
            foreach (double step in steps)
            {
                pr.TryTopN(baseLinks, baseRatings, testRatings, maxIds.Item1, 50, step, 1e-6);
                Console.WriteLine();
            }
            pr.TryTopN(baseLinks, baseRatings, testRatings, maxIds.Item1, 50, 0.9, 1e-6);
        }

        public static void MatrixFactorizationTopNTest(int f = 10)
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            var baseSamples = Tools.RandomSelectNegativeSamples(baseRatings, 4, true);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId, f);
            model.TrySGDForTopN(baseSamples, testRatings, 100, 0.02, 0.01, 0.9);
        }

        // kdd 2016, poi recommendation
        public static void MatrixFactorizationTopNTestTemp(int f = 10)
        {
            List<Rating> baseRatings = Tools.GetRatings(@"D:\study\papers\kdd 2016\Rating_Train");
            List<Rating> testRatings = Tools.GetRatings(@"D:\study\papers\kdd 2016\Rating_Test");

            var baseSamples = Tools.RandomSelectNegativeSamples(baseRatings, 3, true);

            var max1 = Tools.GetMaxUserIdAndItemId(baseRatings);
            var max2 = Tools.GetMaxUserIdAndItemId(testRatings);

            MatrixFactorization model = new MatrixFactorization(2550 + 1, 13473 + 1, f);
            model.TrySGDForTopN(baseSamples, testRatings, 100, 0.02, 0.01, 0.9);
        }

        public static void SLIMTemp(int f = 10)
        {
            List<Rating> baseRatings = Tools.GetRatings(@"D:\study\papers\kdd 2016\Rating_Train");
            List<Rating> testRatings = Tools.GetRatings(@"D:\study\papers\kdd 2016\Rating_Test");

            var baseSamples = Tools.RandomSelectNegativeSamples(baseRatings, 3, true);

            var max1 = Tools.GetMaxUserIdAndItemId(baseRatings);
            var max2 = Tools.GetMaxUserIdAndItemId(testRatings);

            SLIM model = new SLIM(2550 + 1, 13473 + 1);
            model.TryLeastSquare(baseRatings, testRatings, 100, 0.01, 0.001);
        }

        public static void MatrixFactorizationTopNTest()
        {
            double testSize = 0.125;
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

        public static void SLIMTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            SLIM knn = new SLIM(MaxUserId, MaxItemId);
            knn.TryLeastSquare(baseRatings, testRatings, 100, 0.01, 0.001);
        }

        public static void FISMrmseTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            FISMrmse f = new FISMrmse(MaxUserId, MaxItemId, 10);
            //f.TrySGDv2(baseRatings, testRatings, 200, 2, 0.002, 1.0, 0.5, 2e-4, 1e-4, 1e-2);  // the literature
            f.TrySGDv2(baseRatings, testRatings, 200, 3, 0.005, 1, 0.9, 0.001, 0.001, 0.001);

            // parameters given by Hong,2017.12.28
            //learnrate = 0.00001
            //fismauc.rho = 0.5
            //fismauc.alpha = 0.9
            //fismauc.gamma = 0.1
            //factor.number = 10
            //user.regularization = 0.001
            //item.regularization = 0.001
            //bias.regularization = 0.001
        }

        public static void FISMaucTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            FISMauc f = new FISMauc(MaxUserId, MaxItemId, 10);
            f.TrySGD(baseRatings, testRatings, 300, 2);
        }
    }
}
