using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    public class MatrixEntry<TValue>
    {
        /// <summary>
        /// The current row index.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// The current column index.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// The value at the current index (row, column).
        /// </summary>
        public TValue Value { get; set; }


        public MatrixEntry() {}

        public MatrixEntry(int row, int column, TValue value)
        {
            Row = row;
            Column = column;
            Value = value;
        }
    }

    /// <summary>
    /// Extensions for MatrixEntry class.
    /// </summary>
    public static class MatrixEntryExtensions
    {
        /// <summary>
        /// Transform to a sparse matrix.
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static SparseMatrix<double> ToSparseMatrix(this List<MatrixEntry<double>> entries)
        {
            SparseMatrix<double> matrix = new SparseMatrix<double>();
            foreach (var e in entries)
            {
                if (!matrix.HasEntry(e.Row, e.Column))
                {
                    matrix.Add(e.Row, e.Column, e.Value);
                }
            }
            return matrix;
        }

        /// <summary>
        /// Transform to a reversed sparse matrix.
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static SparseMatrix<double> ToReverseSparseMatrix(this List<MatrixEntry<double>> entries)
        {
            SparseMatrix<double> matrix = new SparseMatrix<double>();
            foreach (var e in entries)
            {
                if (!matrix.HasEntry(e.Column, e.Row))
                {
                    matrix.Add(e.Column, e.Row, e.Value);
                }
            }
            return matrix;
        }
    }

}
