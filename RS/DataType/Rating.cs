using System;
using System.Collections.Generic;


namespace RS.DataType
{
    /// <summary>
    /// class Rating
    /// </summary>
    public class Rating : IComparable
    {
        public int UserId = 0;
        public int ItemId = 0;
        public double Score = 0;
        public double Confidence = 1.0;
        public string Timestamp { get; set; }
        public int TimeInterval { get; set; }    // Example: 24 hours of a day.

        public Rating() { }

        public Rating(int userId, int itemId, double score)
        {
            this.UserId = userId;
            this.ItemId = itemId;
            this.Score = score;
        }

        public Rating(int userId, int itemId, double score, string timeStamp)
        {
            this.UserId = userId;
            this.ItemId = itemId;
            this.Score = score;
            this.Timestamp = timeStamp;
        }

        public DateTime GetTime()
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(this.Timestamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }

        // desc
        public static int Compare(Rating x, Rating y)
        {
            if (x.Score < y.Score)
            {
                return 1;
            }
            else if (x.Score == y.Score)
            {
                return 0;
            }
            else
            {
                return -1;
            }

        }

        public int CompareTo(object obj)
        {
            try
            {
                Rating r = obj as Rating;

                if (this.Score < r.Score)
                {
                    return 1;
                }
                else if (this.Score == r.Score)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }

    /// <summary>
    /// class RatingComparer
    /// </summary>
    public class RatingComparer : IComparer<Rating>
    {
        // Desc
        public int Compare(Rating x, Rating y)
        {
            if (x.Score > y.Score)
            {
                return -1;
            }
            else if (x.Score == y.Score)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
