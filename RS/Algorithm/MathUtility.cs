using System;

namespace RS.Algorithm
{
    public static class MathUtility
    {
        public static double RandomGaussian()
        {
            // Joseph L. Leva: A fast normal Random number generator
            double u, v, x, y, Q;
            Random random = new Random();
            do
            {
                do
                {
                    u = random.NextDouble();
                } while (u == 0.0);

                v = 1.7156 * (random.NextDouble() - 0.5);
                x = u - 0.449871;
                y = Math.Abs(v) + 0.386595;
                Q = x * x + y * (0.19600 * y - 0.25472 * x);
                if (Q < 0.27597)
                {
                    break;
                }
            } while ((Q > 0.27846) || ((v * v) > (-4.0 * u * u * System.Math.Log(u))));
            return v / u;
        }

        public static double RandomGaussian(double mean, double stdev)
        {
            if ((stdev == 0.0) || (double.IsNaN(stdev)))
            {
                return mean;
            }
            else
            {
                return mean + stdev * RandomGaussian();
            }
        }

        public static double Norm(double[] vector)
        {
            double norm = 0.0;
            int len = vector.Length;

            for (int i = 0; i < len; i++)
            {
                norm += (vector[i] * vector[i]);
            }
            return Math.Sqrt(norm);
        }

        public static double Norm(double[,] matrix)
        {
            double norm = 0.0;
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    norm += (matrix[i, j] * matrix[i, j]);
                }
            }
            return Math.Sqrt(norm);
        }

        public static double[,] RandomUniform(int rows, int columns)
        {
            double[,] matrix = new double[rows, columns];
            Random r = new Random();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = r.NextDouble();
                }
            }
            return matrix;
        }

        // return values range from 0 to 1, and multiples with factor
        public static double[,] RandomUniform(int rows, int columns, double factor)
        {
            double[,] matrix = new double[rows, columns];
            Random r = new Random();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = r.NextDouble() * factor;
                }
            }
            return matrix;
        }

        public static double[,] RandomGaussian(int rows, int columns, double mean, double stdev)
        {
            double[,] matrix = new double[rows, columns];
            Random r = new Random();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = RandomGaussian(mean, stdev);  // Random filled by 2-d Gaussian
                }
            }
            return matrix;
        }

        public static double[,] T(this double [,] matrix)
        {
            if (matrix == null)
            {
                return null;
            }
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            double[,] _matrix = new double[columns, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    _matrix[j, i] = matrix[i, j];
                }
            }
            return _matrix;
        }

        /// <summary>
        /// Pairwise similarity of Jaccard.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] Jaccard(double[,] matrix)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] similarity = new double[M, M];    // Here, it's a symmetrical matrix
                                                        
            // Step 1. Upper triangular matrix
            for (int i = 0; i < M - 1; i++)
            {
                for (int j = i + 1; j < M; j++)
                {
                    // cos(i, j): cosine similarity of user i and user j
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    int numerator = 0;    // numerator = | i and j |
                    int denominator = 0; // denominator = |i or j|
                    for (int item = 0; item < N; item++)
                    {
                        if (matrix[i, item] > 0 && matrix[j, item] > 0)
                            numerator++;

                        if (matrix[i, item] > 0 || matrix[j, item] > 0)
                            denominator++;
                    }
                    if (0 < denominator)
                    {
                        similarity[i, j] = numerator * 1.0 / (denominator - numerator);
                    }
                }
            }

            // Step 2. Copy to lower triangular matrix
            for (int i = 0; i < M - 1; i++)
            {
                for (int j = i + 1; j < M; j++)
                {
                    similarity[j, i] = similarity[i, j];
                }
            }
            //for (int i = 0; i < M; i++)
            //{
            //    similarity[i, i] = 1.0;
            //}
            return similarity;
        }

        /// <summary>
        /// Pairwise similarity of Cosine.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] Cosine(double[,] matrix)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] similarity = new double[M, M];    // Maybe a symmetrical matrix, and sometime not. Here, it's a symmetrical matrix

            // Step 1. Upper triangular matrix
            for (int i = 0; i < M - 1; i++)
            {
                for (int j = i + 1; j < M; j++)
                {
                    // cos(i, j): cosine similarity of user i and user j
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    double numerator = 0;   // numerator = i * j
                    double denominatorI = 0; // denominator = |i|*|j|
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
                    double denominator = System.Math.Sqrt(denominatorI * denominatorJ);
                    if (0 < denominator)
                    {
                        similarity[i, j] = numerator / denominator;
                    }
                        
                }
            }

            // Step 2. Copy to lower triangular matrix
            for (int i = 0; i < M - 1; i++)
            {
                for (int j = i + 1; j < M; j++)
                {
                    similarity[j, i] = similarity[i, j];
                }
            }
            for (int i = 0; i < M; i++)
            {
                similarity[i, i] = 1.0;
            }

            return similarity;
        }

        /// <summary>
        /// Pairwise similarity of corrected Cosine.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] CorrectedCosine(double[,] matrix)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] similarity = new double[M, M];    // Here, it's a symmetrical matrix

            // Step 1. 计算每个用户的在已打项目上的平均评分
            double[] averageRating = new double[M];
            for (int i = 0; i < M; i++)
            {
                double sum = 0.0;
                int count = 0; // i评分的项目个数

                for (int item = 1; item < N; item++)
                {
                    if (matrix[i, item] > 0)
                    {
                        count++;
                        sum += matrix[i, item];
                    }
                }

                if (count > 0)
                    averageRating[i] = sum / count;
            }

            // Step 2. Upper triangular matrix
            for (int i = 0; i < M - 1; i++)
            {
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
                            double numerator1 = matrix[i, item] - averageRating[i];
                            double numerator2 = matrix[j, item] - averageRating[j];
                            numerator += numerator1 * numerator2;
                        }

                        if (matrix[i, item] > 0)
                            denominatorI += System.Math.Pow(matrix[i, item] - averageRating[i], 2.0);

                        if (matrix[j, item] > 0)
                            denominatorJ += System.Math.Pow(matrix[j, item] - averageRating[j], 2.0);
                    }

                    double denominator = System.Math.Sqrt(denominatorI * denominatorJ);
                    if (0 < denominator)
                        similarity[i, j] = numerator / denominator;
                }
            }

            // Step 3. Copy to lower triangular matrix
            // 不在上面直接对称写，一定程度上可以提高程序的执行效率
            for (int i = 0; i < M - 1; i++)
                for (int j = i + 1; j < M; j++)
                    similarity[j, i] = similarity[i, j];

            //for (int i = 0; i < M; i++)
            //    similarity[i, i] = 1.0;

            return similarity;
        }

        /// <summary>
        /// Pairwise similarity of Pearson Correlation.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static double[,] PearsonCorrelation(double[,] ratings)
        {
            int M = ratings.GetLength(0);
            int N = ratings.GetLength(1);

            double[,] similarity = new double[M, M];    // Here, it's a symmetrical matrix

            // Step 1. 计算每个用户的在已打项目上的平均评分
            double[] averageRating = new double[M];
            for (int i = 0; i < M; i++)
            {
                double sum = 0.0;
                int count = 0; // i评分的项目个数
                for (int item = 1; item < N; item++)
                {
                    if (ratings[i, item] > 0)
                    {
                        count++;
                        sum += ratings[i, item];
                    }
                }

                if (count > 0)
                    averageRating[i] = sum / count;
            }

            // Step 2. Upper triangular matrix
            for (int i = 0; i < M - 1; i++)
            {
                for (int j = i + 1; j < M; j++)
                {
                    // vector i: set of items that user i rated
                    // vector j: set of items that user j rated
                    double numerator = 0;
                    double denominatorI = 0;
                    double denominatorJ = 0;

                    for (int item = 1; item < N; item++)
                    {
                        if (ratings[i, item] > 0 && ratings[j, item] > 0)
                        {
                            double numerator1 = ratings[i, item] - averageRating[i];
                            double numerator2 = ratings[j, item] - averageRating[j];
                            numerator += numerator1 * numerator2;

                            denominatorI += Math.Pow(numerator1, 2.0);
                            denominatorJ += Math.Pow(numerator2, 2.0);
                        }
                    }

                    double denominator = Math.Sqrt(denominatorI * denominatorJ);
                    if (0 < denominator)
                        similarity[i, j] = numerator / denominator;
                }
            }

            // Step 3. Copy to lower triangular matrix
            for (int i = 0; i < M - 1; i++)
                for (int j = i + 1; j < M; j++)
                    similarity[j, i] = similarity[i, j];

            //for (int i = 0; i < M; i++)
            //    similarity[i, i] = 1.0;

            return similarity;
        }

        /// <summary>
        /// Pairwise distance between pairs of ratings.
        /// refs. http://www.cnblogs.com/renge-blogs/p/6308912.html
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="distance">default as Cosine. [Jaccard | Cosine | CorrectedCosine | PearsonCorrelation]</param>
        /// <returns></returns>
        public static double[,] PairwiseDistance(double[,] matrix, string distance = "Cosine")
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
            else if (distance == "CorrectedCosine" || distance == "correctedcosine")
            {
                return CorrectedCosine(matrix);
            }
            else if (distance == "PearsonCorrelation" || distance == "pearson")
            {
                return PearsonCorrelation(matrix);
            }
            return Cosine(matrix);
        }

        /// <summary>
        /// Return the inverse of a matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] Inverse(double[,] matrix)
        {
            int m = 0;
            int n = 0;
            m = matrix.GetLength(0);
            n = matrix.GetLength(1);
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
            double[,] NI = new double[m, n];
            for (int x = 0; x < m; x++)
            {
                for (int y = n; y < 2 * n; y++)
                {
                    NI[x, y - n] = array[x, y];
                }
            }
            return NI;
        }
        
    }
}
