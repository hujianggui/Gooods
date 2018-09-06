using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.Data
{
    /// <summary>
    /// class Rating
    /// </summary>
    public class Rating
    {
        public int UserId = 0;
        public int ItemId = 0;
        public double Score = 0;
        public double Confidence = 1.0; // cui in ICDM 2008, Yifan Hu and Yehuda Koren and Chris Volinsky, p263
        public string Timestamp { get; set; }
        public int TimeInterval { get; set; }    // Example: 24 hours of a day.

        public Rating() { }

        public Rating(int userId, int itemId, double score)
        {
            UserId = userId;
            ItemId = itemId;
            Score = score;
        }

        public Rating(int userId, int itemId, double score, string timeStamp)
        {
            UserId = userId;
            ItemId = itemId;
            Score = score;
            Timestamp = timeStamp;
        }

        public DateTime GetTime()
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(Timestamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }
    }
}
