using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Gooods.DataType;

namespace Gooods.CollaborativeFiltering
{
    /// <summary>
    /// class UserKNNPredictor, rating prediction
    /// </summary>
    public class UserKNNPredictor
    {
        public UserKNNPredictor()
        {
            Console.WriteLine(GetType().Name);
        }

        public SparseVector<List<VectorEntry<double>>> TryTopN(SparseMatrix<double> userItemRatingMatrix, SparseVector<List<VectorEntry<double>>> userUserSimilarity, int N = 20)
        {
            SparseVector<List<VectorEntry<double>>> recommendations = new SparseVector<List<VectorEntry<double>>>();


            return recommendations;
        }

        public double TryRatingPrediction(SparseMatrix<double> userItemRatingMatrix, SparseVector<double> userAverageRating, 
            SparseVector<List<VectorEntry<double>>> userUserSimilarity, int userId, int itemId)
        {
            double numerator = 0.0;
            double denominator = 0.0;
            double pui = userAverageRating[userId];
            foreach (var entry in userUserSimilarity[userId])   // similar users, v in Su
            {
                if (userItemRatingMatrix.HasEntry(entry.Index, itemId)) // if user v has rated item i
                {
                    numerator += (userItemRatingMatrix[entry.Index][itemId] - userAverageRating[entry.Index]) * entry.Value;
                    denominator += entry.Value;
                    pui += numerator / denominator;
                }
            }
            return pui;
        }

        public void EvaluateRatingPrediction(SparseMatrix<double> userItemRatingMatrix,
            SparseVector<List<VectorEntry<double>>> userUserSimilarity, List<MatrixEntry<double>> test)
        {
            SparseVector<double> userAverageRating = userItemRatingMatrix.RowAverage();
            double mae = 0, rmse = 0;
            foreach (var entry in test)   
            {
                if (userItemRatingMatrix.ContainsKey(entry.Row))    // if has user 
                {
                    double pui = TryRatingPrediction(userItemRatingMatrix, userAverageRating, userUserSimilarity, entry.Row, entry.Column);
                    // Console.WriteLine("{0},{1},{2}", entry.Row, entry.Column, pui);
                    double error = entry.Value - pui;
                    mae += Math.Abs(error);
                    rmse += error * error;
                }
            }

            if (test.Count > 0)
            {
                mae /= test.Count;
                rmse = Math.Sqrt(rmse / test.Count);
            }
            Console.WriteLine("{0},{1}", mae, rmse);
        }

        public void TryPrediction(List<MatrixEntry<double>> train, List<MatrixEntry<double>> test, int K = 160)
        {
            SparseMatrix<double> userItemRatingMatrix = train.ToSparseMatrix();
            SparseVector<List<VectorEntry<double>>> userUserSimilarity = userItemRatingMatrix.Jaccard(K);
            EvaluateRatingPrediction(userItemRatingMatrix, userUserSimilarity, test);
        }
    }

   
}