using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gooods.DataType;

namespace Gooods.Data
{
    /// <summary>
    /// Extensions for rating class.
    /// </summary>
    public static class RatingExtensions
    {
        /// <summary>
        /// Transform to list of matrix entries.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static List<MatrixEntry<double>> ToMatrixEntries(this List<Rating> ratings)
        {
            List<MatrixEntry<double>> entries = new List<MatrixEntry<double>>();
            foreach(var r in ratings)
            {
                MatrixEntry<double> e = new MatrixEntry<double>(r.UserId, r.ItemId, r.Score);
                entries.Add(e);
            }
            return entries;
        }

        /// <summary>
        /// Transform to a sparse matrix.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static SparseMatrix<double> ToSparseMatrix(this List<Rating> ratings)
        {
            SparseMatrix<double> matrix = new SparseMatrix<double>();
            foreach (var r in ratings)
            {
                if (!matrix.HasEntry(r.UserId, r.ItemId))
                {
                    matrix.Add(r.UserId, r.ItemId, r.Score);
                }
            }
            return matrix;
        }

        /// <summary>
        /// update user and item index to zero-based,  designed for movielens.
        /// userId -= userId
        /// imteId -= itemId
        /// </summary>
        /// <param name="ratings"></param>
        public static void UpdateIndexesToZeroBased(List<Rating> ratings)
        {
            foreach (Rating r in ratings)
            {
                r.UserId -= 1;
                r.ItemId -= 1;
            }
        }

    }
}
