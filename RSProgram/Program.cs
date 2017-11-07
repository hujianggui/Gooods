using System;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;
using RS.Algorithm;
using RS.Data.Utility;

namespace RSProgram
{
    class Program
    {

        public static void MatrixFactorization(Dictionary<string, string> pairs)
        {
            string separator = "\t";
            if (pairs.Keys.Contains("separator"))
            {
                separator = pairs["separator"];
            }

            List<Rating> baseRatings = Tools.GetRatings(pairs["train"], separator);
            List<Rating> testRatings = Tools.GetRatings(pairs["test"], separator);

            int maxUserId = System.Math.Max(baseRatings.Max(r => r.UserId), testRatings.Max(r => r.UserId));
            int maxItemId = System.Math.Max(baseRatings.Max(r => r.ItemId), testRatings.Max(r => r.ItemId));

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            int f = 10;
            if (pairs.Keys.Contains("f"))
            {
                f = Int32.Parse(pairs["f"]);
            }

            int epochs = 100;
            if (pairs.Keys.Contains("epochs"))
            {
                epochs = Int32.Parse(pairs["epochs"]);
            }

            double gamma = 0.01;
            if (pairs.Keys.Contains("gamma"))
            {
                gamma = Double.Parse(pairs["gamma"]);
            }

            double lambda = 0.01;
            if (pairs.Keys.Contains("lambda"))
            {
                lambda = Double.Parse(pairs["lambda"]);
            }

            double min_rating = 1.0;
            if (pairs.Keys.Contains("min_rating"))
            {
                min_rating = Double.Parse(pairs["min_rating"]);
            }

            double max_rating = 5.0;
            if (pairs.Keys.Contains("max_rating"))
            {
                max_rating = Double.Parse(pairs["max_rating"]);
            }

            MatrixFactorization model = new MatrixFactorization(maxUserId, maxItemId, f);
            model.TrySGD(baseRatings, testRatings, epochs, gamma, lambda, min_rating, max_rating);
        }

        public static void BiasedMatrixFactorization(Dictionary<string, string> pairs)
        {
            string separator = "\t";
            if (pairs.Keys.Contains("separator"))
            {
                separator = pairs["separator"];
            }

            List<Rating> baseRatings = Tools.GetRatings(pairs["train"], separator);
            List<Rating> testRatings = Tools.GetRatings(pairs["test"], separator);

            int maxUserId = System.Math.Max(baseRatings.Max(r => r.UserId), testRatings.Max(r => r.UserId));
            int maxItemId = System.Math.Max(baseRatings.Max(r => r.ItemId), testRatings.Max(r => r.ItemId));

            Tools.UpdateIndexesToZeroBased(baseRatings);
            Tools.UpdateIndexesToZeroBased(testRatings);

            int f = 10;
            if (pairs.Keys.Contains("f"))
            {
                f = Int32.Parse(pairs["f"]);
            }

            int epochs = 100;
            if (pairs.Keys.Contains("epochs"))
            {
                epochs = Int32.Parse(pairs["epochs"]);
            }

            double gamma = 0.01;
            if (pairs.Keys.Contains("gamma"))
            {
                gamma = Double.Parse(pairs["gamma"]);
            }

            double lambda = 0.01;
            if (pairs.Keys.Contains("lambda"))
            {
                lambda = Double.Parse(pairs["lambda"]);
            }

            double min_rating = 1.0;
            if (pairs.Keys.Contains("min_rating"))
            {
                min_rating = Double.Parse(pairs["min_rating"]);
            }

            double max_rating = 5.0;
            if (pairs.Keys.Contains("max_rating"))
            {
                max_rating = Double.Parse(pairs["max_rating"]);
            }

            BiasedMatrixFactorization model = new BiasedMatrixFactorization(maxUserId, maxItemId, f);
            model.TrySGD(baseRatings, testRatings, epochs, gamma, lambda, min_rating, max_rating);
        }

        static void Main(string[] args)
        {
            CommandArgs commandArg = CommandLine.Parse(args);
            Dictionary<string, string> pairs = commandArg.ArgPairs;

            if (!pairs.Keys.Contains("model"))
            {
                Console.WriteLine(@".\RSProgram -model [MatrixFactorization | BiasedMatrixFactorization]");
                return;
            }

            if (pairs["model"] == "MatrixFactorization")
            {
                if (!(pairs.Keys.Contains("train") && pairs.Keys.Contains("test")))
                {
                    Console.WriteLine("MatrixFactorization");
                    Console.WriteLine("    -train training_file -test test_file [-separator '\t']");
                    Console.WriteLine("    [-f latent_features] [-epochs max_epochs(100)] [-gamma learning_rate(0.01)]");
                    Console.WriteLine("    [-lambda regularization_parameter(0.01)] [-min_rating minimum(1.0)] [-max_rating maximum(5.0)]");
                    return;
                }
                MatrixFactorization(pairs);
            }

            if (pairs["model"] == "BiasedMatrixFactorization")
            {
                if (!(pairs.Keys.Contains("train") && pairs.Keys.Contains("test")))
                {
                    Console.WriteLine("BiasedMatrixFactorization");
                    Console.WriteLine("    -train training_file -test test_file [-separator '\t']");
                    Console.WriteLine("    [-f latent_features] [-epochs max_epochs(100)] [-gamma learning_rate(0.01)]");
                    Console.WriteLine("    [-lambda regularization_parameter(0.01)] [-min_rating minimum(1.0)] [-max_rating maximum(5.0)]");
                    return;
                }
                BiasedMatrixFactorization(pairs);
            }

        }
    }
}
