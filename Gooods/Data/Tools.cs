using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gooods.Data
{
    public class Tools
    {
        /// <summary>
        /// Get ratings from a csv file
        /// userId \t itemId \t score
        /// </summary>
        /// <param name="file">rating file</param>
        /// <param name="separator">default as '\t'</param>
        /// <returns></returns>
        public static List<Rating> GetRatings(string file, string separator = "\t")
        {
            if (!File.Exists(file))
            {
                throw new ArgumentException("File doesn't exist: " + file);
            }

            StreamReader reader = new StreamReader(file);
            List<Rating> ratings = new List<Rating>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] elements = line.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (elements.Length == 3)
                {
                    int uid = Int32.Parse(elements[0]);
                    int iid = Int32.Parse(elements[1]);
                    double rate = Double.Parse(elements[2]);
                    Rating r = new Rating(uid, iid, rate);
                    ratings.Add(r);
                }
                else if (elements.Length == 4)
                {
                    int uid = Int32.Parse(elements[0]);
                    int iid = Int32.Parse(elements[1]);
                    double rate = Double.Parse(elements[2]);
                    Rating r = new Rating(uid, iid, rate, elements[3]);
                    ratings.Add(r);
                }
                else if (elements.Length == 2)
                {
                    int uid = Int32.Parse(elements[0]);
                    int iid = Int32.Parse(elements[1]);
                    Rating r = new Rating(uid, iid, 1.0);
                    ratings.Add(r);
                }
            }
            reader.Close();
            return ratings;
        }

        /// <summary>
        /// Write ratings to a given csv file
        /// </summary>
        /// <param name="ratings"></param>
        /// <param name="toFile"></param>
        /// <param name="separator"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public static void WriteRatings(List<Rating> ratings, string toFile, string separator = ",", bool append = false, string encoding = "GB2312")
        {
            if (ratings == null)
            {
                throw new ArgumentNullException();
            }

            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            foreach (Rating r in ratings)
            {
                writer.WriteLine("{0}{1}{2}{3}{4}", r.UserId, separator, r.ItemId, separator, r.Score);
            }
            writer.Close();
        }

 


    }
}
