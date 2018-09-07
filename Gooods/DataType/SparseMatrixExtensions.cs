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

        /// <summary>
        /// Pairwise similarity using Jaccard.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static SparseVector<List<VectorEntry<double>>> Jaccard(this SparseMatrix<double> matrix, int K = 80)
        {
            SparseVector<List<VectorEntry<double>>> S = new SparseVector<List<VectorEntry<double>>>();
            List<int> rows = matrix.GetRowKeyList();
            Parallel.ForEach(rows, u => {
                var Nu = matrix[u];     // items rated by user u
                List<VectorEntry<double>> su = new List<VectorEntry<double>>(); // similarity from user u to other users.
                foreach (int v in matrix.Keys)
                {
                    if (u == v) { continue; }
                    var Nv = matrix[v];   // items rated by user v
                    int numerator = 0;    // numerator = | i and j |
                    int denominator = 0;  // denominator = |i union j| = | i | + | j | - | i and j |
                    foreach (int item in Nu.Keys)
                    {
                        if (Nv.ContainsKey(item))
                        {
                            numerator++;
                        }
                    }
                    denominator = Nu.Count + Nv.Count - numerator;
                    su.Add(new VectorEntry<double>(v, numerator * 1.0 / denominator));
                }
                var sortedSimilarities = su.OrderByDescending(e => e.Value).ToList().GetRange(0, Math.Min(K, su.Count));
                lock(S)
                {
                    S.Add(u, sortedSimilarities);
                }
            });
            return S;
        }


    }

    /// <summary>
    /// Test for SparseMatrixExtensions class.
    /// </summary>
    public class SparseMatrixExtensionsTest
    {
        public static void Run()
        {
            SparseMatrix<double> matrix = new SparseMatrix<double>();
            matrix.Add(0, 0, 1);
            matrix.Add(0, 2, 3);
            matrix.Add(0, 3, 4);
            matrix.Add(1, 0, 3);
            matrix.Add(1, 2, 5);
            matrix.Add(2, 2, 1);
            matrix.Add(2, 3, 3);
            Console.WriteLine("A sparse matrix");
            Console.WriteLine(matrix.ToString());

            Console.WriteLine("Jaccard similarity");
            Console.WriteLine(matrix.Jaccard().ToString());

            Console.WriteLine("Jaccard_v2 similarity");
            var S = matrix.Jaccard();
            foreach(int r in S.Keys)
            {
                List<VectorEntry<double>> row = S[r];
                Console.Write("{0}", r);
                foreach(var e in row)
                {
                    Console.Write(" {0}:{1:N2}", e.Index, e.Value);
                }
                Console.WriteLine();
            }           

        }




    }

}
