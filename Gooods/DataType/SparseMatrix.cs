using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    /// <summary>
    /// 2D sparse hash table
    /// </summary>
    /// <typeparam name="TRowKey"></typeparam>
    /// <typeparam name="TColumnKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SparseTable2D<TRowKey, TColumnKey, TValue> : Dictionary<TRowKey, Dictionary<TColumnKey, TValue>>
    {
        protected Dictionary<TColumnKey, int> ColumnNames = new Dictionary<TColumnKey, int>();
        
        public virtual bool HasEntry(TRowKey r, TColumnKey c)
        {
            if (ContainsKey(r))
            {
                if (this[r].ContainsKey(c))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void Add(TRowKey r, TColumnKey c, TValue v)
        {
            if (!ContainsKey(r))
            {
                this[r] = new Dictionary<TColumnKey, TValue>();
            }

            if (!this[r].ContainsKey(c))
            {
                this[r].Add(c, v);
            }

            if (!ColumnNames.ContainsKey(c))
            {
                ColumnNames.Add(c, ColumnNames.Count);
            }
        }

        public virtual int ColumnIndex(TColumnKey c)
        {
            if (ColumnNames.ContainsKey(c))
            {
                return ColumnNames[c];
            }
            return -1;
        }

        /// <summary>
        /// Get list of rows
        /// </summary>
        /// <returns></returns>
        public virtual List<TRowKey> GetRowKeyList()
        {
            return new List<TRowKey>(Keys);
        }

        /// <summary>
        /// Get list of columns
        /// </summary>
        /// <returns></returns>
        public virtual List<TColumnKey> GetColumnKeyList()
        {
            return new List<TColumnKey>(ColumnNames.Keys);
        }

        /// <summary>
        /// </summary>
        /// <returns>matrix content</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var _r in Keys)
            {
                var row = this[_r];
                builder.Append(_r);
                foreach (var _c in row.Keys)
                {
                    builder.AppendFormat(" {0}:{1:N2}", _c, row[_c]);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Sparse matrix
    /// </summary>
    /// <typeparam name="TValue">the matrix element type, must have a default constructor/value</typeparam>
    public class SparseMatrix<TValue> : SparseTable2D<int, int, TValue> where TValue : new()
    {
        public int NumberOfRows { get { return Keys.Count; } }
        public int NumberOfColumns { get { return ColumnNames.Keys.Count; } }

        /// <summary>
        /// Matrix transpose
        /// </summary>
        /// <returns>A new transposed matrix</returns>
        public SparseMatrix<TValue> Transpose()
        {
            SparseMatrix<TValue> transpose = new SparseMatrix<TValue>();
            foreach (var _r in Keys)
            {
                var row = this[_r];
                foreach (var _c in row.Keys)
                {
                    transpose.Add(_c, _r, row[_c]);
                }                
            }
            return transpose;
        }
    }

    /// <summary>
    /// Sparse matrix test
    /// </summary>
    public class SparseMatrixTest
    {
        protected SparseMatrix<double> SparseMatrix = new SparseMatrix<double>();
        protected SparseTable2D<string, string, double> SparseTable2D = new SparseTable2D<string, string, double>();

        public double MM()
        {
            long usedMemory = Process.GetCurrentProcess().WorkingSet64;
            return usedMemory * 1.0 / 1024 / 1024;
        }

        public void Initialize(int rows = 100, int columns = 100)
        {
            Random random = Random.GetInstance();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (!SparseMatrix.HasEntry(i, j))
                    {
                        SparseMatrix.Add(i, j, random.NextDouble());
                    }
                }
            }
        }

        public double Sum()
        {
            double _sum = 0.0;
            foreach (var _r in SparseMatrix.Keys)
            {
                var row = SparseMatrix[_r];
                foreach (var _c in row.Keys)
                {
                    _sum += row[_c];
                }
            }
            return _sum;
        }

        public SparseMatrix<double> Transpose()
        {
            return SparseMatrix.Transpose();
        }

        public void InitializeSparseTable2D(int rows = 100, int columns = 100)
        {
            Random random = Random.GetInstance();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (!SparseTable2D.HasEntry(i.ToString(), j.ToString()))
                    {
                        SparseTable2D.Add(i.ToString(), j.ToString(), random.NextDouble());
                    }
                }
            }
        }

        public double SumSparseTable2D()
        {
            double _sum = 0.0;
            foreach (var _r in SparseTable2D.Keys)
            {
                var row = SparseTable2D[_r];
                foreach (var _c in row.Keys)
                {
                    _sum += row[_c];
                }
            }
            return _sum;
        }

        public static void Run()
        {
            const int rows = 4, columns = 5;
            SparseMatrixTest smt = new SparseMatrixTest();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.WriteLine("#rows,{0},#columns,{1}", smt.SparseMatrix.NumberOfRows, smt.SparseMatrix.NumberOfColumns);
            
            smt.Initialize(rows, columns);
            Console.WriteLine("#rows,{0},#columns,{1}", smt.SparseMatrix.NumberOfRows, smt.SparseMatrix.NumberOfColumns);

            stopWatch.Stop();
            Console.WriteLine("MM,{0:N4}MB,Initialize(),{1}", smt.MM(), stopWatch.Elapsed);

            stopWatch.Reset();
            stopWatch.Start();

            smt.Sum();

            stopWatch.Stop();
            Console.WriteLine("MM,{0:N4}MB,Sum(),{1}", smt.MM(), stopWatch.Elapsed);


            stopWatch.Reset();
            stopWatch.Start();

            var tranposedMatrix = smt.Transpose();

            stopWatch.Stop();
            Console.WriteLine("MM,{0:N4}MB,Transpose(),{1}", smt.MM(), stopWatch.Elapsed);

            stopWatch.Reset();
            stopWatch.Start();

            smt.InitializeSparseTable2D(rows, columns);

            stopWatch.Stop();
            Console.WriteLine("MM,{0:N4}MB,InitializeSparseTable2D(),{1}", smt.MM(), stopWatch.Elapsed);

            stopWatch.Reset();
            stopWatch.Start();

            smt.SumSparseTable2D();

            stopWatch.Stop();
            Console.WriteLine("MM,{0:N4}MB,SumSparseTable2D(),{1}", smt.MM(), stopWatch.Elapsed);
        }
    }
}
