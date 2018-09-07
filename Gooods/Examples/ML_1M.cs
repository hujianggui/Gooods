using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gooods.Data;
using Gooods.CollaborativeFiltering;

namespace Gooods.Examples
{
    public class ML_1M
    {
        public static string DefalultDirectory = @"D:\data\movielens\ml-1m\";

        public static string DefaultRatingFile = DefalultDirectory + @"ratings.dat";
        public static string DefaultItemFile = DefalultDirectory + @"movies.dat";
        public static string DefaultUserFile = DefalultDirectory + @"users.dat";

        public static string BaseRatingFile = DefalultDirectory + @"u1.base";
        public static string TestRatingFile = DefalultDirectory + @"u1.test";

        public static void MeanFillingTest()
        {
            var baseRatings = Tools.GetRatings(BaseRatingFile, "::").ToMatrixEntries();
            var testRatings = Tools.GetRatings(TestRatingFile, "::").ToMatrixEntries();

            MeanFilling.GlobalMean(baseRatings, testRatings, true);
            MeanFilling.UserMean(baseRatings, testRatings, true);
            MeanFilling.ItemMean(baseRatings, testRatings, true);
        }

        public static void UserKNNPredictorTest()
        {
            var baseRatings = Tools.GetRatings(BaseRatingFile, "::").ToMatrixEntries();
            var testRatings = Tools.GetRatings(TestRatingFile, "::").ToMatrixEntries();

            UserKNNPredictor ucf = new UserKNNPredictor();
            ucf.TryPrediction(baseRatings, testRatings, 400);

        }

    }
}
