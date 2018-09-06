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

        public void TryPrediction(List<MatrixEntry<double>> train, List<MatrixEntry<double>> test, int k = 80)
        {
            SparseMatrix<double> userItemRatingMatrix = train.ToSparseMatrix();
            SparseMatrix<double> userUserSimilarity = userItemRatingMatrix.Jaccard();

            Console.WriteLine(userUserSimilarity.ToString());

        }
    }

   
}