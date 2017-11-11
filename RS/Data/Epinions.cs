using System;
using System.Collections;
using System.Collections.Generic;

using RS.Data.Utility;
using RS.DataType;
using RS.CollaborativeFiltering;


namespace RS.Data
{
    public static class Epinions
    {
        public static string DefalultDirectory = @"D:\data\dataset_dm\epinions\";
        public static string DefaultRatingFile = DefalultDirectory + @"ratings_data.txt";
        public static string DefaultLinkFile = DefalultDirectory + @"trust_data.txt";

        public static string BaseRatingFile = DefalultDirectory + @"ratings.base";
        public static string TestRatingFile = DefalultDirectory + @"ratings.test";

        public static int MaxUserId = 49290;   // 40163 users(1-49289)
        public static int MaxItemId = 139738;  // 139738 items(1-139738)


        public static void MeanFillingTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            var t0 = MeanFilling.TryGlobalMean(baseRatings, testRatings);
            var t1 = MeanFilling.TryUserMean(baseRatings, testRatings);
            var t2 = MeanFilling.TryItemMean(baseRatings, testRatings);
            Console.WriteLine(t0);
            Console.WriteLine(t1);
            Console.WriteLine(t2);
        }

        public static void MatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.01, 0.01, 0.98);
        }

        public static void AlternatingLeastSquaresTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            AlternatingLeastSquares model = new AlternatingLeastSquares(MaxUserId, MaxItemId, 10);
            model.TryALS(baseRatings, testRatings, 100, 0.01);
        }

        public static void BiasedMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.01, 0.01, 0.95);
        }

        public static void SVDPlusPlusTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            SVDPlusPlus model = new SVDPlusPlus(MaxUserId, MaxItemId);

            model.TrySGD(baseRatings, testRatings, 1000);
        }

        public static void FriendMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");
            List<Link> links = Tools.GetLinks(DefaultLinkFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            FriendMatrixFactorization model = new FriendMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, links, 2.6, 100, 0.01, 0.01, 0.9);
        }

        public static void FriendBiasedMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");
            List<Link> links = Tools.GetLinks(DefaultLinkFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            FriendBiasedMatrixFactorization model = new FriendBiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, links, 0.02, 100, 0.01, 0.01, 0.96);
        }

        public static void AdaptiveFriendBiasedMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");
            List<Link> links = Tools.GetLinks(DefaultLinkFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            AdaptiveFriendBiasedMatrixFactorization model = new AdaptiveFriendBiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, links, 100, 0.01, 0.01, 0.96);
        }


        public static void AdaptiveFriendMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");
            List<Link> links = Tools.GetLinks(DefaultLinkFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            AdaptiveFriendMatrixFactorization model = new AdaptiveFriendMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, links, 100, 0.01, 0.01, 0.95);
        }

        public static void SocialMFTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");
            List<Link> links = Tools.GetLinks(DefaultLinkFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            SocialMF model = new SocialMF(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, links, 200, 0.02, 0.9, 0.01, 0.01, 0.01);
        }

        public static void MatrixFactorizationTopNTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            var baseSamples = Tools.RandomSelectNegativeSamples(baseRatings, 1, true);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId, 10);
            model.TrySGDForTopN(baseSamples, testRatings, 100, 0.02, 0.01, 0.9, 0, 1);
        }

        public static void AdaptiveFriendMatrixFactorizationTopNTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");
            List<Link> links = Tools.GetLinks(DefaultLinkFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            var baseSamples = Tools.RandomSelectNegativeSamples(baseRatings, 1, true);

            AdaptiveFriendMatrixFactorization model = new AdaptiveFriendMatrixFactorization(MaxUserId, MaxItemId, 10);
            model.TrySGDForTopN(baseSamples, testRatings, links, 100, 0.02, 0.01, 0.9, 1, 5);
        }

    }
}
