using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gooods.DataType;
using System.Collections;

namespace Gooods.CollaborativeFiltering
{
    /// <summary>
    /// Mean Filling methods for rating prediction.
    /// </summary>
    public class MeanFilling
    {
        /// <summary>
        /// Use global mean to fill unknown ratings.
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static Tuple<double, double> GlobalMean(List<MatrixEntry<double>> train, List<MatrixEntry<double>> test, bool verbose = false)
        {
            double miu = train.Average(e => e.Value);
            // Prediction and evaluation
            double mae = 0.0;
            double rmse = 0.0;

            foreach (var r in test)
            {
                double error = miu - r.Value;
                mae += Math.Abs(error);
                rmse += error * error;
            }

            if (test.Count > 0)
            {
                mae /= test.Count;
                rmse = Math.Sqrt(rmse / test.Count);
            }

            if (verbose)
            {
                Console.WriteLine("GlobalMean,mae,{0},rmse,{1}", mae, rmse);
            }

            return Tuple.Create(mae, rmse);
        }

        /// <summary>
        /// Use user mean to fill unknown ratings.
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static Tuple<double, double> UserMean(List<MatrixEntry<double>> train, List<MatrixEntry<double>> test, bool verbose = false)
        {
            double miu = 0.0;           // global mean

            Hashtable table = new Hashtable();
            foreach (var r in train)
            {
                miu += r.Value;

                if (!table.ContainsKey(r.Row))
                {
                    table.Add(r.Row, new List<MatrixEntry<double>>() { r });
                }
                else
                {
                    List<MatrixEntry<double>> li = (List<MatrixEntry<double>>)table[r.Row];
                    li.Add(r);
                }
            }

            if (train.Count > 1)
            {
                miu /= train.Count;
            }

            Hashtable userMeanRatings = new Hashtable();    // key: userId, value: mean rating of this user.
            foreach (int uId in table.Keys)
            {
                List<MatrixEntry<double>> li = (List<MatrixEntry<double>>)table[uId];
                userMeanRatings.Add(uId, li.Average(r => r.Value));
            }

            // Prediction and evaluation
            double mae = 0.0;
            double rmse = 0.0;

            foreach (var r in test)
            {
                double error = 0.0;
                if (table.ContainsKey(r.Row))
                {
                    error = (double)userMeanRatings[r.Row] - r.Value;
                }
                else
                {
                    error = miu - r.Value;
                }
                mae += Math.Abs(error);
                rmse += error * error;
            }

            if (test.Count > 0)
            {
                mae /= test.Count;
                rmse = Math.Sqrt(rmse / test.Count);
            }

            if (verbose)
            {
                Console.WriteLine("UserMean,mae,{0},rmse,{1}", mae, rmse);
            }
            return Tuple.Create(mae, rmse);
        }

        /// <summary>
        /// Use item mean to fill unknown ratings.
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static Tuple<double, double> ItemMean(List<MatrixEntry<double>> train, List<MatrixEntry<double>> test, bool verbose = false)
        {
            double miu = 0.0;           // global mean

            Hashtable table = new Hashtable();
            foreach (MatrixEntry<double> r in train)
            {
                miu += r.Value;

                if (!table.ContainsKey(r.Column))
                {
                    table.Add(r.Column, new List<MatrixEntry<double>>() { r });
                }
                else
                {
                    List<MatrixEntry<double>> li = (List<MatrixEntry<double>>)table[r.Column];
                    li.Add(r);
                }
            }

            if (train.Count > 1)
            {
                miu /= train.Count;
            }

            Hashtable itemMeanRatings = new Hashtable();    // key: userId, value: mean rating of this user.
            foreach (int iId in table.Keys)
            {
                List<MatrixEntry<double>> li = (List<MatrixEntry<double>>)table[iId];
                itemMeanRatings.Add(iId, li.Average(r => r.Value));
            }

            // Prediction and evaluation
            double mae = 0.0;
            double rmse = 0.0;

            foreach (MatrixEntry<double> r in test)
            {
                double error = 0.0;
                if (table.ContainsKey(r.Column))
                {
                    error = (double)itemMeanRatings[r.Column] - r.Value;
                }
                else
                {
                    error = miu - r.Value;
                }
                mae += System.Math.Abs(error);
                rmse += error * error;
            }

            if (test.Count > 0)
            {
                mae /= test.Count;
                rmse = Math.Sqrt(rmse / test.Count);
            }
            if (verbose)
            {
                Console.WriteLine("ItemMean,mae,{0},rmse,{1}", mae, rmse);
            }
            return Tuple.Create(mae, rmse);
        }
    }
}
