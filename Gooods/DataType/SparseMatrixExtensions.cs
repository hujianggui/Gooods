using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    public static class SparseMatrixExtensions
    {
        /// <summary>
        /// Row average.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static SparseVector<double> RowAverage(this SparseMatrix<double> matrix)
        {
            SparseVector<double> average = new SparseVector<double>();
            foreach (var r in matrix.Keys)
            {
                var row = matrix[r];
                double a = row.Values.Average();
                average.Add(r, a);
            }
            return average;
        }



    }
}
