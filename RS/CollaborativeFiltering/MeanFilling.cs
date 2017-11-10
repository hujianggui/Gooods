using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;

namespace RS.CollaborativeFiltering
{
    public class MeanFilling
    {
        /// <summary>
        /// user mean to fill unknown ratings
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <returns>MAE, RMSE</returns>
        public static Tuple<double, double> TryUserMean(List<Rating> train, List<Rating> test)
        { 
            double miu = 0.0;           // global mean

            Hashtable table = new Hashtable();
            foreach(Rating r in train)
            {
                miu += r.Score;

                if (!table.ContainsKey(r.UserId))
                {
                    table.Add(r.UserId, new List<Rating>() { r });
                }
                else
                {
                     List<Rating> li = (List<Rating>)table[r.UserId];
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
                List<Rating> li = (List<Rating>)table[uId];
                userMeanRatings.Add(uId, li.Average(r => r.Score));
            }

            // Prediction and evaluation
            double mae = 0.0;
            double rmse = 0.0;

            foreach (Rating r in test)
            {
                double error = 0.0;
                if (table.ContainsKey(r.UserId))
                {
                    error = (double)userMeanRatings[r.UserId] - r.Score;
                }
                else
                {
                    error = miu - r.Score;
                }
                mae += Math.Abs(error);
                rmse += error * error;
            }

            if (test.Count > 0)
            {
                mae /= test.Count;
                rmse = Math.Sqrt(rmse / test.Count);
            }

            return Tuple.Create(mae, rmse);
        }

        /// <summary>
        /// item mean to fill unknown ratings
        /// </summary>
        /// <param name="train"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public static Tuple<double, double> TryItemMean(List<Rating> train, List<Rating> test)
        {
            double miu = 0.0;           // global mean

            Hashtable table = new Hashtable();
            foreach (Rating r in train)
            {
                miu += r.Score;

                if (!table.ContainsKey(r.ItemId))
                {
                    table.Add(r.ItemId, new List<Rating>() { r });
                }
                else
                {
                    List<Rating> li = (List<Rating>)table[r.ItemId];
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
                List<Rating> li = (List<Rating>)table[iId];
                itemMeanRatings.Add(iId, li.Average(r => r.Score));
            }

            // Prediction and evaluation
            double mae = 0.0;
            double rmse = 0.0;

            foreach (Rating r in test)
            {
                double error = 0.0;
                if (table.ContainsKey(r.ItemId))
                {
                    error = (double)itemMeanRatings[r.ItemId] - r.Score;
                }
                else
                {
                    error = miu - r.Score;
                }
                mae += System.Math.Abs(error);
                rmse += error * error;
            }

            if (test.Count > 0)
            {
                mae /= test.Count;
                rmse = Math.Sqrt(rmse / test.Count);
            }

            return Tuple.Create(mae, rmse);
        }


    }
}
