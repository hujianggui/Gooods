using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Gooods.DataType;

namespace Gooods.CollaborativeFiltering
{
    /// <summary>
    /// class UserKNN, rating prediction
    /// </summary>
    public class UserKNN
    {
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="ratings"></param>
        ///// <param name="numberOfUsers"></param>
        ///// <returns></returns>
        //protected double[] CalcuateUserMeanRating(List<Rating> ratings, int numberOfUsers)
        //{
        //    double[] userMeanRating = new double[numberOfUsers];

        //    var query = from r in ratings
        //                group r by r.UserId into g
        //                select new
        //                {
        //                    userId = g.Key,
        //                    meanScore = g.Average(item => item.Score)
        //                };

        //    foreach (var q in query)
        //    {
        //        userMeanRating[q.userId] = q.meanScore;
        //    }

        //    return userMeanRating;
        //}

        //protected double Predict(double[] userMeanRating, double[,] similarities, double[,] ratings, int userId, int itemId)
        //{
        //    double score = userMeanRating[userId];
        //    int M = similarities.GetLength(0);
        //    double numerator = 0.0;
        //    double denominator = 0.0;
        //    for (int i = 0; i < M; i++)
        //    {
        //        if (ratings[i, itemId] > 0 && similarities[userId, i] > 0)
        //        {
        //            numerator += similarities[userId, i] * (ratings[i, itemId] - userMeanRating[i]);
        //            denominator += similarities[userId, i];
        //        }
        //    }
        //    score += (denominator > 0 ? numerator / denominator : 0);
        //    return score;
        //}

        //protected Tuple<double, double> EvaluateMaeRmse(double[] userMeanRating, double[,] similarities, double[,] ratings, List<Rating> test)
        //{
        //    double mae = 0.0;
        //    double rmse = 0.0;

        //    foreach (Rating r in test)
        //    {
        //        double pui = Predict(userMeanRating, similarities, ratings, r.UserId, r.ItemId);
        //        double error = pui - r.Score;
        //        mae += Math.Abs(error);
        //        rmse += error * error;
        //    }

        //    if (test.Count > 0)
        //    {
        //        mae /= test.Count;
        //        rmse = Math.Sqrt(rmse / test.Count);
        //    }

        //    return Tuple.Create(mae, rmse);
        //}

        //public void TryMaeRmse(List<Rating> train, List<Rating> test)
        //{
        //    int maxUserId = Math.Max(train.AsParallel().Max(r => r.UserId), test.AsParallel().Max(r => r.UserId));
        //    int maxItemId = Math.Max(train.AsParallel().Max(r => r.ItemId), test.AsParallel().Max(r => r.ItemId));
        //    double[] userMeanRating = CalcuateUserMeanRating(train, maxUserId + 1);

        //    double[,] trainMatrix = Tools.Transform(train, maxUserId + 1, maxItemId + 1);
        //    double[,] similarities = MathUtility.PairwiseDistance(trainMatrix, "PearsonCorrelation");   // PearsonCorrelation | Cosine

        //    var result = EvaluateMaeRmse(userMeanRating, similarities, trainMatrix, test);
        //    Console.WriteLine("{0},{1}", result.Item1, result.Item2);
        //}



    }

   
}