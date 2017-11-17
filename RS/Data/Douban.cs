using RS.CollaborativeFiltering;
using RS.Data.Utility;
using RS.DataType;
using System;
using System.Collections.Generic;



namespace RS.Data
{
    /// <summary>
    /// douban data collection
    /// WSDM '11, Ma
    /// http://dl.dropbox.com/u/17517913/Douban.zip
    /// </summary>
    public class Douban
    {
        public static string DefalultDirectory = @"D:\data\dataset_dm\Douban\";
        public static string DefaultRatingFile = DefalultDirectory + @"uir.index";
        public static string DefaultLinkFile = DefalultDirectory + @"social.index";

        public static string BaseRatingFile = DefalultDirectory + @"uir.base";
        public static string TestRatingFile = DefalultDirectory + @"uir.test";

        public static int MaxUserId = 129490;
        public static int MaxItemId = 58541;


        public static void MeanFillingTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            MeanFilling.TryGlobalMean(baseRatings, testRatings, true);
            MeanFilling.TryUserMean(baseRatings, testRatings, true);
            MeanFilling.TryItemMean(baseRatings, testRatings, true);
        }

        public static void MatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            MatrixFactorization model = new MatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.02, 0.98);
        }

        public static void BiasedMatrixFactorizationTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.02, 0.98);
        }

        public static void SVDPlusPlusTest()
        {
            List<Rating> baseRatings = Tools.GetRatings(BaseRatingFile, " ");
            List<Rating> testRatings = Tools.GetRatings(TestRatingFile, " ");

            SVDPlusPlus model = new SVDPlusPlus(MaxUserId, MaxItemId);
            model.TrySGD(baseRatings, testRatings, 100, 0.005, 0.02, 0.98);
        }
    }
}
