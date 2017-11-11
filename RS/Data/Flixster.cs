using System;
using System.Collections.Generic;

using RS.Data.Utility;
using RS.DataType;
using RS.CollaborativeFiltering;

namespace RS.Data
{
    public static class Flixster
    {
        public static string DefalultDirectory = @"D:\data\dataset_dm\flixster\";
        public static string DefaultRatingFile = DefalultDirectory + @"ratings.txt";
        public static string DefaultLinkFile = DefalultDirectory + @"links.txt";

        public static string BaseRatingFile = DefalultDirectory + @"ratings.base";
        public static string TestRatingFile = DefalultDirectory + @"ratings.test";

        public static int MaxUserId = 1049511;   // 147612 users in (6-1049508),  max user id in link (1049491,1049511)
        public static int MaxItemId = 66726;   //  48794 items (1-66726)


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

        public static void MatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.02, 0.98, 0.5);
        }

        public static void BiasedMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.02, 0.98, 0.5);
        }

        public static void SVDPlusPlusTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            SVDPlusPlus model = new SVDPlusPlus(MaxUserId, MaxItemId);

            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.02, 0.98, 0.5);
        }

        public static void AdaptiveFriendMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile);
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile);
            List<Link> links = Tools.GetLinks(DefaultLinkFile);

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);
            Tools.UpdateIndexesToZeroBased(links);

            AdaptiveFriendMatrixFactorization model = new AdaptiveFriendMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, links, 100, 0.005, 0.02, 0.98);
        }
        

    }
}
