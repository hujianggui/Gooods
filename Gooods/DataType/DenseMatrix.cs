using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    /// <summary>
    /// Dense matrix
    /// </summary>
    /// <typeparam name="TValue">the matrix element type, must have a default constructor/value</typeparam>
    public class DenseMatrix<TValue> where TValue : new()
    {
        public TValue[,] data { get; protected set; }

        public virtual TValue this[int r, int c]
        {
            get { return data[r, c]; }
            set { data[r, c] = value; }
        }

        public int NumberOfRows { get { return data.GetLength(0); } }
        public int NumberOfColumns { get { return data.GetLength(1); } }

        /// <summary>
        /// Construct a dense matrix by given rows * columns.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public DenseMatrix(int rows, int columns)
        {
            data = new TValue[rows, columns];
        }

        /// <summary>
        /// Construct a dense matrix from a 2d array.
        /// </summary>
        /// <param name="matrix"></param>
        public DenseMatrix(TValue[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            data = new TValue[rows, columns];
            Array.Copy(matrix, data, matrix.Length);
        }

        /// <summary>
        /// Construct a dense matrix from another dense matrix.
        /// </summary>
        /// <param name="another"></param>
        public DenseMatrix(DenseMatrix<TValue> another) : this(another.data)
        {
        }

        /// <summary>
        /// Transpose a matrix
        /// </summary>
        /// <returns>Transposed matrix</returns>
        public DenseMatrix<TValue> Transpose()
        {
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);

            DenseMatrix<TValue> _matrix = new DenseMatrix<TValue>(columns, rows);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    _matrix[j, i] = data[i, j];
                }
            }
            return _matrix;
        }

        /// <summary>
        /// Make a string from the matrix content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                builder.AppendFormat("{0:N2}", data[i, 0]);
                for (int j = 1; j < columns; j++)
                {
                    builder.AppendFormat("\t{0:N2}", data[i, j]);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        /// <summary>
        /// Write the dense matrix to file
        /// </summary>
        /// <param name="f"></param>
        public void ToStream(string f)
        {
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);
            StreamWriter writer = new StreamWriter(f, false);
            for (int i = 0; i < rows; i++)
            {
                writer.Write("{0:N2}", data[i, 0]);
                for (int j = 1; j < columns; j++)
                {
                    writer.Write("\t{0:N2}", data[i, j]);
                }
                writer.WriteLine();
            }
            writer.Close();
        }
    }

    /// <summary>
    /// Test for DenseMatrix class.
    /// </summary>
    public class DenseMatrixTest
    {
        public static void Run()
        {
            DenseMatrix<double> dm = new DenseMatrix<double>(new double[,] { { 1, 0, 3, 4 }, { 3, 0, 5, 0 }, { 0, 0, 1, 3 } });
            Console.WriteLine("Original matrix");
            Console.WriteLine(dm.ToString());

            DenseMatrix<double> transpose = dm.Transpose();
            Console.WriteLine("Transposed matrix");
            Console.WriteLine(transpose.ToString());

            DenseMatrix<double> another = new DenseMatrix<double>(dm);
            Console.WriteLine("Another matrix");
            Console.WriteLine(another.ToString());
        }
    }
}
