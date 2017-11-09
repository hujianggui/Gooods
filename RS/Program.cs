using System;

using RS.Data;


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
            //ML_100k.MatrixFactorizationTopNTest();
            //ML_100k.AlternatingLeastSquaresTopNTest();

            // ml-1m
            //ML_1M.UserKNNTest();
            //ML_1M.MatrixFactorizationTest();
            //ML_1M.BiasedMatrixFactorizationTest(0.1);
            //ML_1M.SVDPlusPlusTest();

            //ML_1M.UserKNNv2Test();
            //ML_1M.ItemKNNv2Test();
            //ML_1M.MatrixFactorizationTopNTest();
            //ML_1M.BiasedMatrixFactorizationTopNTest();

            // ml-10m
            //ML_10M.MatrixFactorizationTest(0.2);
            //ML_10M.BiasedMatrixFactorizationTest(0.2);

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
            YelpE.BiasedMatrixFactorizationTest(0.1);

        }
    }
}
