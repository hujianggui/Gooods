using System;

using RS.Data;
using System.Collections.Generic;
using RS.DataType;
using RS.Data.Utility;

namespace RS
{
    class Program
    {
        static void Main(string[] args)
        {
            // ml-100k
            //ML_100k.MeanFillingTest();
            //ML_100k.UserKNNTest(0.1);
            //ML_100k.MatrixFactorizationTest();
            //ML_100k.BiasedMatrixFactorizationTest();
            //ML_100k.SVDPlusPlusTest();
            //ML_100k.AlternatingLeastSquaresTest();
            //ML_100k.EuclideanEmbeddingTest();

            //ML_100k.UserKNNv2Test();
            //ML_100k.ItemKNNv2Test();
            ML_100k.MatrixFactorizationTopNTest(10); Console.WriteLine();
            ML_100k.MatrixFactorizationTopNTest(20); Console.WriteLine();
            ML_100k.MatrixFactorizationTopNTest(50); Console.WriteLine();
            ML_100k.MatrixFactorizationTopNTest(100); Console.WriteLine();
            ML_100k.MatrixFactorizationTopNTest(200); Console.WriteLine();
            ML_100k.MatrixFactorizationTopNTest(500); Console.WriteLine();
            //ML_100k.AlternatingLeastSquaresTopNTest();
            //ML_100k.SLIMTest();

            // ml-1m
            //ML_1M.UpdateDataInformation();
            //ML_1M.MeanFillingTest();
            //ML_1M.UserKNNTest();
            //ML_1M.MatrixFactorizationTest();
            //ML_1M.BiasedMatrixFactorizationTest(0.1);
            //ML_1M.SVDPlusPlusTest();

            //ML_1M.UserKNNv2Test();
            //ML_1M.ItemKNNv2Test();
            //ML_1M.MatrixFactorizationTopNTest();
            //ML_1M.BiasedMatrixFactorizationTopNTest();

            // ml-10m
            //ML_10M.MeanFillingTest();
            //ML_10M.MatrixFactorizationTest();
            //ML_10M.BiasedMatrixFactorizationTest(0.2);

            // book-crossing 2004
            //BookCrossing.Preprocessing();

            // Epinions 2006
            //Epinions.MeanFillingTest();
            //Epinions.MatrixFactorizationTest();
            //Epinions.AlternatingLeastSquaresTest();
            //Epinions.BiasedMatrixFactorizationTest();
            //Epinions.SVDPlusPlusTest();

            //Epinions.FriendMatrixFactorizationTest();
            //Epinions.AdaptiveFriendMatrixFactorizationTest();
            //Epinions.FriendBiasedMatrixFactorizationTest();
            //Epinions.AdaptiveFriendBiasedMatrixFactorizationTest();
            //Epinions.SocialMFTest();


            //Epinions.MatrixFactorizationTopNTest();
            //Epinions.AdaptiveFriendMatrixFactorizationTopNTest();

            // Flixster 2009
            //Flixster.MeanFillingTest();
            //Flixster.MatrixFactorizationTest();
            //Flixster.BiasedMatrixFactorizationTest();
            //Flixster.SVDPlusPlusTest();
            //Flixster.AdaptiveFriendMatrixFactorizationTest();


            // Douban 2011
            //Douban.MeanFillingTest();
            //Douban.MatrixFactorizationTest();
            //Douban.BiasedMatrixFactorizationTest();
            //Douban.SVDPlusPlusTest();

            // Yelp 2014

            // Yelp 2015
            //YelpE.MatrixFactorizationTest(0.1);
            //YelpE.BiasedMatrixFactorizationTest(0.1);

        }
    }
}
