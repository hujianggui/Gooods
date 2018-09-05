using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.DataType
{
    /// <summary>
    /// Dense vector
    /// </summary>
    /// <typeparam name="TValue">the matrix element type, must have a default constructor/value</typeparam>
    public class DenseVector<TValue> where TValue : new()
    {
        public TValue[] data { get; protected set; }

        public virtual TValue this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public int Length { get { return data.Length; } }

        /// <summary>
        /// Construct a dense vector by a given length.
        /// </summary>
        /// <param name="length"></param>
        public DenseVector(int length)
        {
            data = new TValue[length];
        }

        /// <summary>
        /// Construct a dense vector from an array.
        /// </summary>
        /// <param name="vector"></param>
        public DenseVector(TValue[] vector)
        {
            data = new TValue[vector.Length];
            Array.Copy(vector, data, vector.Length);
        }

        /// <summary>
        /// Construct a dense vector from another dense vector.
        /// </summary>
        /// <param name="another"></param>
        public DenseVector(DenseVector<TValue> another) : this(another.data)
        {
        }

        /// <summary>
        /// Make a string from the vector content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.AppendFormat("{0:N2}\t", data[i]);                
                builder.AppendLine();                             
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Extensions for DenseVector class.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Norm of a dense vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static double Norm(this DenseVector<double> vector)
        {
            double sumOfSquare = vector.data.AsParallel().Sum(v => v * v);
            return Math.Sqrt(sumOfSquare);
        }

        /// <summary>
        /// The sum of squared differences between two vectors.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="another"></param>
        /// <returns></returns>
        public static double SumOfSquaredDifference(this DenseVector<double> vector, DenseVector<double> another)
        {
            double sum = 0.0;
            int length = vector.Length;
            for (int i = 0; i < length; i++)
            {
                double error = vector[i] - another[i];
                sum += (error * error);
            }
            return sum;
        }

    }

    /// <summary>
    /// Test for vector extions
    /// </summary>
    public class VectorExtensionsTest
    {
        public static void Run()
        {
            double[] array = new double[] { 1, 2, 3, 4, 5};
            DenseVector<double> vector = new DenseVector<double>(array);
            Console.WriteLine(vector.ToString());

            Console.WriteLine("Norm,{0}", vector.Norm());

            DenseVector<double> vector2 = new DenseVector<double>(vector);
            vector2[2] = 20;
            Console.WriteLine(vector2.ToString());

            double ssd = vector2.SumOfSquaredDifference(vector);
            Console.WriteLine("SumOfSquaredDifference,{0}", ssd);
        }
    }

}
