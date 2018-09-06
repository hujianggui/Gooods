using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    /// <summary>
    /// Matrix extensions for dense matrix.
    /// </summary>
    public static class DenseMatrixExtensions
    {
        /// <summary>
        /// Initialize the matrix using Uniform distribution.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="low"></param>
        /// <param name="high">values range in [low, high]</param>
        public static void Uniform(this DenseMatrix<double> matrix, double low = 0, double high = 1)
        {
            double interval = high - low;
            int rows = matrix.NumberOfRows;
            int columns = matrix.NumberOfColumns;
            Random r = Random.GetInstance();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = r.NextDouble() * interval + low;
                }
            }
        }

        /// <summary>
        /// Initialize values range from 0 to 1, and multiples with factor
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="factor"></param>
        public static void Uniform(this DenseMatrix<double> matrix, double factor)
        {
            int rows = matrix.NumberOfRows;
            int columns = matrix.NumberOfColumns;
            Random r = Random.GetInstance();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = r.NextDouble() * factor;
                }
            }
        }

        /// <summary>
        /// Random filled by 2-d Gaussian.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="mean"></param>
        /// <param name="stdev"></param>
        public static void Gaussian(this DenseMatrix<double> matrix, double mean = 0.0, double stdev = 1.0)
        {
            int rows = matrix.NumberOfRows;
            int columns = matrix.NumberOfColumns;
            Random r = Random.GetInstance();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = r.Gaussian(mean, stdev);  // Random filled by 2-d Gaussian
                }
            }
        }


        /// <summary>
        /// Norm of a matrix.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double Norm(this DenseMatrix<double> matrix)
        {
            double norm = 0.0;
            int rows = matrix.NumberOfRows;
            int columns = matrix.NumberOfColumns;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    norm += (matrix[i, j] * matrix[i, j]);
                }
            }
            return Math.Sqrt(norm);
        }

        /// <summary>
        /// Inverse of a matrix.
        /// </summary>
        /// <param name="matrix">The inversed matrix</param>
        /// <returns></returns>
        public static DenseMatrix<double> Inverse(this DenseMatrix<double> matrix)
        {
            int m = matrix.NumberOfRows;
            int n = matrix.NumberOfColumns;
            double[,] array = new double[2 * m + 1, 2 * n + 1];
            for (int k = 0; k < 2 * m + 1; k++)  // Initialize
            {
                for (int t = 0; t < 2 * n + 1; t++)
                {
                    array[k, t] = 0.00000000;
                }
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    array[i, j] = matrix[i, j];
                }
            }

            for (int k = 0; k < m; k++)
            {
                for (int t = n; t <= 2 * n; t++)
                {
                    if ((t - k) == m)
                    {
                        array[k, t] = 1.0;
                    }
                    else
                    {
                        array[k, t] = 0;
                    }
                }
            }
            //得到逆矩阵
            for (int k = 0; k < m; k++)
            {
                if (array[k, k] != 1)
                {
                    double bs = array[k, k];
                    array[k, k] = 1;
                    for (int p = k + 1; p < 2 * n; p++)
                    {
                        array[k, p] /= bs;
                    }
                }
                for (int q = 0; q < m; q++)
                {
                    if (q != k)
                    {
                        double bs = array[q, k];
                        for (int p = 0; p < 2 * n; p++)
                        {
                            array[q, p] -= bs * array[k, p];
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            DenseMatrix<double> NI = new DenseMatrix<double>(m, n);
            for (int x = 0; x < m; x++)
            {
                for (int y = n; y < 2 * n; y++)
                {
                    NI[x, y - n] = array[x, y];
                }
            }
            return NI;
        }

        /// <summary>
        /// Row average, note that zero entries are not counted.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static DenseVector<double> RowAverage(this DenseMatrix<double> matrix)
        {
            int M = matrix.NumberOfRows;
            int N = matrix.NumberOfColumns;
            DenseVector<double> average = new DenseVector<double>(M);
            Parallel.For(0, M, i => {
                double sum = 0.0;
                int count = 0;
                for (int j = 0; j < N; j++)
                {
                    if (matrix[i, j] > 0)
                    {
                        sum += matrix[i, j];
                        count++;
                    }
                }
                if (count > 0)
                {
                    average[i] = sum / count;
                }
            });
            return average;
        }

        /// <summary>
        /// Multiplication of two rows from two matrices respectively.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <param name="other"></param>
        /// <param name="otherRow"></param>
        /// <returns></returns>
        public static double RowMultiplication(this DenseMatrix<double> matrix, int row, DenseMatrix<double> other, int otherRow)
        {
            double sum = 0.0;
            for(int i = 0; i < matrix.NumberOfColumns; i++)
            {
                sum += matrix[row, i] * other[otherRow, i];
            }
            return sum;
        }

        /// <summary>
        /// Pairwise similarity using Jaccard.
        /// </summary>
        /// <param name="matrix">row - row Jaccard simiarity</param>
        /// <returns></returns>
        public static DenseMatrix<double> Jaccard(this DenseMatrix<double> matrix)
        {
            int M = matrix.NumberOfRows;
            int N = matrix.NumberOfColumns;
            DenseMatrix<double> similarity = new DenseMatrix<double>(M, M);    // Here, it's a symmetrical matrix

            // Upper triangular matrix
            Parallel.For(0, M - 1, i => {
                for (int j = i + 1; j < M; j++)
                {
                    // cos(i, j): cosine similarity of user i and user j
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    int numerator = 0;    // numerator = | i and j |
                    int denominator = 0; // denominator = |i union j| = | i | + | j | - | i and j |
                    for (int item = 0; item < N; item++)
                    {
                        if (matrix[i, item] > 0 && matrix[j, item] > 0)
                            numerator++;

                        if (matrix[i, item] > 0)
                            denominator++;

                        if (matrix[j, item] > 0)
                            denominator++;
                    }
                    if (0 < denominator)
                    {
                        similarity[i, j] = numerator * 1.0 / (denominator - numerator);
                        similarity[j, i] = similarity[i, j];    // Copy to lower triangular matrix
                    }
                }
            });            
            return similarity;
        }

        /// <summary>
        /// Pairwise similarity using Cosine.
        /// </summary>
        /// <param name="matrix">row - row Cosine simiarity</param>
        /// <returns></returns>
        public static DenseMatrix<double> Cosine(this DenseMatrix<double> matrix)
        {
            int M = matrix.NumberOfRows;
            int N = matrix.NumberOfColumns;
            DenseMatrix<double> similarity = new DenseMatrix<double>(M, M);   // Maybe a symmetrical matrix, and sometime not. Here, it's a symmetrical matrix

            // Upper triangular matrix
            Parallel.For(0, M - 1, i => {
                for (int j = i + 1; j < M; j++)
                {
                    // cos(i, j): cosine similarity of user i and user j
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    double numerator = 0;   // numerator = i * j
                    double denominatorI = 0; // denominator = sqrt (sum(i * i) * sum(j * j)）
                    double denominatorJ = 0;
                    for (int item = 0; item < N; item++)
                    {
                        if (matrix[i, item] > 0 && matrix[j, item] > 0)
                        {
                            numerator += matrix[i, item] * matrix[j, item];
                        }

                        if (matrix[i, item] > 0)
                        {
                            denominatorI += matrix[i, item] * matrix[i, item];
                        }

                        if (matrix[j, item] > 0)
                        {
                            denominatorJ += matrix[j, item] * matrix[j, item];
                        }
                    }
                    double denominator = Math.Sqrt(denominatorI * denominatorJ);
                    if (0 < denominator)
                    {
                        similarity[i, j] = numerator / denominator;
                        similarity[j, i] = similarity[i, j];    // Copy to lower triangular matrix
                    }

                }
            });            
            return similarity;
        }


        /// <summary>
        /// Pairwise similarity of Adjusted Cosine.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>row - row Adjusted Cosine simiarity</returns>
        public static DenseMatrix<double> AdjustedCosine(this DenseMatrix<double> matrix)
        {
            int M = matrix.NumberOfRows;
            int N = matrix.NumberOfColumns;
            DenseMatrix<double> similarity = new DenseMatrix<double>(M, M);    // Here, it's a symmetrical matrix

            // Step 1. average value of each row
            DenseVector<double> rowAverage = matrix.RowAverage();

            // Step 2. Upper triangular matrix
            Parallel.For(0, M - 1, i => {
                for (int j = i + 1; j < M; j++)
                {
                    // cos(i, j): cosine similarity of user i and user j
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    double numerator = 0;
                    double denominatorI = 0;
                    double denominatorJ = 0;
                    for (int item = 0; item < N; item++)
                    {
                        if (matrix[i, item] > 0 && matrix[j, item] > 0)
                        {
                            double numerator1 = matrix[i, item] - rowAverage[i];
                            double numerator2 = matrix[j, item] - rowAverage[j];
                            numerator += numerator1 * numerator2;
                        }
                        if (matrix[i, item] > 0)
                        {
                            denominatorI += Math.Pow(matrix[i, item] - rowAverage[i], 2.0);
                        }
                        if (matrix[j, item] > 0)
                        {
                            denominatorJ += Math.Pow(matrix[j, item] - rowAverage[j], 2.0);
                        }
                    }
                    double denominator = Math.Sqrt(denominatorI * denominatorJ);
                    if (0 < denominator)
                    {
                        similarity[i, j] = numerator / denominator;
                        similarity[j, i] = similarity[i, j];     //  Copy to lower triangular matrix
                    }
                }
            });
            return similarity;
        }

        /// <summary>
        /// Pairwise similarity using Pearson Correlation Coefficient (PCC).
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns>row - row Pearson Correlation Coefficient simiarity, values in [-1, 1]</returns>
        public static DenseMatrix<double> PearsonCorrelationCoefficient(this DenseMatrix<double> matrix)
        {
            int M = matrix.NumberOfRows;
            int N = matrix.NumberOfColumns;
            DenseMatrix<double> similarity = new DenseMatrix<double>(M, M);     // Here, it's a symmetrical matrix

            // Step 1. average value of each row
            DenseVector<double> rowAverage = matrix.RowAverage();

            // Step 2. Upper triangular matrix
            Parallel.For(0, M - 1, i => {
                for (int j = i + 1; j < M; j++)
                {
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    double numerator = 0;
                    double denominatorI = 0;
                    double denominatorJ = 0;
                    for (int item = 1; item < N; item++)
                    {
                        if (matrix[i, item] > 0 && matrix[j, item] > 0)
                        {
                            double numerator1 = matrix[i, item] - rowAverage[i];
                            double numerator2 = matrix[j, item] - rowAverage[j];
                            numerator += numerator1 * numerator2;

                            denominatorI += numerator1 * numerator1;
                            denominatorJ += numerator2 * numerator2;
                        }
                    }
                    double denominator = Math.Sqrt(denominatorI * denominatorJ);
                    if (0 < denominator)
                    {
                        similarity[i, j] = numerator / denominator;  // Copy to lower triangular matrix
                        similarity[j, i] = similarity[i, j];
                    }
                }
            });
            return similarity;
        }


        /// <summary>
        /// Pairwise distance between pairs of ratings.
        /// refs. http://www.cnblogs.com/renge-blogs/p/6308912.html
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="distance">default as Cosine. [Jaccard | Cosine | CorrectedCosine | PearsonCorrelation]</param>
        /// <returns></returns>
        public static DenseMatrix<double> PairwiseDistance(this DenseMatrix<double> matrix, string distance = "Cosine")
        {
            if (matrix == null)
            {
                return null;
            }
            if (distance == "Jaccard" || distance == "jaccard")
            {
                return Jaccard(matrix);
            }
            else if (distance == "Cosine" || distance == "cosine")
            {
                return Cosine(matrix);
            }
            else if (distance == "AdjustedCosine" || distance == "adjustedcosine")
            {
                return AdjustedCosine(matrix);
            }
            else if (distance == "PearsonCorrelation" || distance == "pearson")
            {
                return PearsonCorrelationCoefficient(matrix);
            }
            return Cosine(matrix);
        }

    }

    /// <summary>
    /// Test MatrixExtensions class
    /// </summary>
    public class MatrixExtensionsTest
    {

        public static void Run()
        {
            DenseMatrix<double> matrix = new DenseMatrix<double>(2, 3);
            matrix.Uniform(1, 5);
            Console.WriteLine("Initial using Uniform distribution");
            Console.WriteLine(matrix.ToString());

            Console.WriteLine("Initial using Gaussian distribution");
            matrix.Gaussian();
            Console.WriteLine(matrix.ToString());

            DenseMatrix<double> toBeInversed = new DenseMatrix<double>(new double[,] { { 1, 2, 3 }, { 2, 2, 1 }, { 3, 4, 3 } });
            Console.WriteLine("Original matrix to be inversed");
            Console.WriteLine(toBeInversed.ToString());

            Console.WriteLine("Inverse matrix");
            Console.WriteLine(toBeInversed.Inverse().ToString());

            DenseMatrix<double> dm = new DenseMatrix<double>(new double[,] { { 1, 0, 3, 4 }, { 3, 0, 5, 0 }, { 0, 0, 1, 3 } });
            Console.WriteLine("Original matrix");
            Console.WriteLine(dm.ToString());

            DenseMatrix<double> jaccardSimilarity = dm.Jaccard();
            Console.WriteLine("Jaccard similarity");
            Console.WriteLine(jaccardSimilarity.ToString());

            DenseMatrix<double> cosineSimilarity = dm.Cosine();
            Console.WriteLine("Cosine similarity");
            Console.WriteLine(cosineSimilarity.ToString());

            Console.WriteLine("Row average (entry > 0)");
            Console.WriteLine(dm.RowAverage().ToString());

            DenseMatrix<double> adjustedCosineSimilarity = dm.AdjustedCosine();
            Console.WriteLine("Adjusted Cosine similarity");
            Console.WriteLine(adjustedCosineSimilarity.ToString());

            DenseMatrix<double> pccSimilarity = dm.PearsonCorrelationCoefficient();
            Console.WriteLine("Pearson Correlation Coefficient similarity");
            Console.WriteLine(pccSimilarity.ToString());
        }


    }


}
