using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using RS.DataType;

namespace RS.Data.Utility
{
    public static class Tools
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

        /// <summary>
        /// Get ratings with timestamp from a csv file
        /// </summary>
        /// <param name="file">rating file</param>
        /// <param name="separator">default as '\t'</param>
        /// <returns></returns>
        public static List<Rating> GetTimedRatings(string file, string separator = "\t")
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

                int uid = Int32.Parse(elements[0]);
                int iid = Int32.Parse(elements[1]);
                double rate = Double.Parse(elements[2]);
                string timestamp = elements[3];

                Rating r = new Rating(uid, iid, rate, timestamp);

                ratings.Add(r);
            }
            reader.Close();
            return ratings;
        }

        /// <summary>
        /// Write timed ratings to a given csv file
        /// </summary>
        /// <param name="ratings"></param>
        /// <param name="toFile"></param>
        /// <param name="separator"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public static void WriteTimedRatings(List<Rating> ratings, string toFile, string separator = ",", bool append = false, string encoding = "GB2312")
        {
            if (ratings == null)
            {
                throw new ArgumentNullException();
            }

            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            foreach (Rating r in ratings)
            {
                writer.WriteLine("{0}{1}{2}{3}{4}{5}{6}", r.UserId, separator, r.ItemId, separator, r.Score, separator, r.Timestamp);
            }
            writer.Close();
        }

        /// <summary>
        /// update user and item index to zero-based,  designed for movielens
        /// userId -= userId
        /// imteId -= itemId
        /// </summary>
        /// <param name="ratings"></param>
        public static void UpdateIndexesToZeroBased(List<Rating> ratings)
        {
            foreach (Rating r in ratings)
            {
                r.UserId -= 1;
                r.ItemId -= 1;
            }
        }

        /// <summary>
        /// update user index to zero-based,  designed for epinions
        /// </summary>
        /// <param name="links"></param>
        public static void UpdateIndexesToZeroBased(List<Link> links)
        {
            foreach (Link l in links)
            {
                l.From -= 1;
                l.To -= 1;
            }
        }

        /// <summary>
        /// Get user id - ratings hash table. 2017.06.10
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static Hashtable GetUserItemsTable(List<Rating> ratings)
        {
            Hashtable userItemsTable = new Hashtable();
            foreach (Rating r in ratings)
            {
                if (userItemsTable.ContainsKey(r.UserId))
                {
                    List<Rating> li = (List<Rating>)userItemsTable[r.UserId];
                    li.Add(r);
                }
                else
                {
                    userItemsTable.Add(r.UserId, new List<Rating>() { r });
                }
            }
            return userItemsTable;
        }

        /// <summary>
        /// Get item id - ratings hash table.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static Hashtable GetItemUsersTable(List<Rating> ratings)
        {
            Hashtable itemUsersTable = new Hashtable();
            foreach (Rating r in ratings)
            {
                if (itemUsersTable.ContainsKey(r.ItemId))
                {
                    List<Rating> li = (List<Rating>)itemUsersTable[r.ItemId];
                    li.Add(r);
                }
                else
                {
                    itemUsersTable.Add(r.ItemId, new List<Rating>() { r });
                }
            }
            return itemUsersTable;
        }

        /// <summary>
        /// Get user id - item id - ratings 2 level hash table.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static MyTable GetRatingTable(List<Rating> ratings)
        {
            MyTable table = new MyTable();
            foreach (Rating r in ratings)
            {
                if (!table.ContainsKey(r.UserId, r.ItemId))
                {
                    table.Add(r.UserId, r.ItemId, r.Score);
                }
            }
            return table;
        }

        /// <summary>
        /// Get item id - user id - ratings 2 level hash table.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static MyTable GetReversedRatingTable(List<Rating> ratings)
        {
            MyTable table = new MyTable();
            foreach (Rating r in ratings)
            {
                if (!table.ContainsKey(r.ItemId, r.UserId))
                {
                    table.Add(r.ItemId, r.UserId, r.Score);
                }
            }
            return table;
        }


        /// <summary>
        /// Get subset of a ranked list by K
        /// </summary>
        /// <param name="rankedItems">Generated recommendation items</param>
        /// <param name="recommendItemsPerUser">K</param>
        /// <returns></returns>
        public static List<Rating> GetSubset(List<Rating> rankedItems, int recommendItemsPerUser)
        {
            Hashtable userRatingsTable = GetUserItemsTable(rankedItems);
            List<Rating> subset = new List<Rating>();
            foreach (int userId in userRatingsTable.Keys)
            {
                List<Rating> li = (List<Rating>)userRatingsTable[userId];
                subset.AddRange(li.GetRange(0, Math.Min(li.Count, recommendItemsPerUser)));
            }
            return subset;
        }


        /// <summary>
        /// Get links from a csv file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="separator">default as comma</param>
        /// <returns></returns>
        public static List<Link> GetLinks(string file, string separator = "\t")
        {
            if (!File.Exists(file))
            {
                throw new ArgumentException("File doesn't exist: " + file);
            }

            StreamReader reader = new StreamReader(file);
            List<Link> links = new List<Link>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] elements = line.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // update
                if (elements.Length == 2)
                {
                    int from = Int32.Parse(elements[0]);
                    int to = Int32.Parse(elements[1]);
                    Link l = new Link(from, to);
                    links.Add(l);
                }
                else if (elements.Length == 3)
                {
                    int from = Int32.Parse(elements[0]);
                    int to = Int32.Parse(elements[1]);
                    double weight = Double.Parse(elements[2]);
                    Link l = new Link(from, to, weight);
                    links.Add(l);
                }
                else if (elements.Length == 4)
                {
                    // ignore elements[3]
                    int from = Int32.Parse(elements[0]);
                    int to = Int32.Parse(elements[1]);
                    double weight = Double.Parse(elements[2]);
                    Link l = new Link(from, to, weight);
                    links.Add(l);
                }
            }
            reader.Close();
            return links;
        }

        /// <summary>
        /// Write links to a csv file.
        /// </summary>
        /// <param name="links"></param>
        /// <param name="toFile"></param>
        /// <param name="separator"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public static void WriteLinks(List<Link> links, string toFile, string separator = ",", bool append = false, string encoding = "GB2312")
        {
            if (links == null)
            {
                throw new ArgumentNullException();
            }

            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            foreach (Link l in links)
            {
                writer.WriteLine("{0}{1}{2}{3}{4}", l.From, separator, l.To, separator, l.Weight);
            }
            writer.Close();
        }


        /// <summary>
        /// Get user id - linked user id - weight table.
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static MyTable GetLinkTable(List<Link> links)
        {
            MyTable table = new MyTable();
            foreach (Link l in links)
            {
                if (!table.ContainsKey(l.From, l.To))
                {
                    table.Add(l.From, l.To, l.Weight);
                }
            }
            return table;
        }

        /// <summary>
        /// Get linked user id - user id - weight table.
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static MyTable GetReversedLinkTable(List<Link> links)
        {
            MyTable table = new MyTable();
            foreach (Link l in links)
            {
                if (!table.ContainsKey(l.To, l.From))
                {
                    table.Add(l.To, l.From, l.Weight);
                }
            }
            return table;
        }

        /// <summary>
        /// Get user id - links hash table, a.k.a. trusters.
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static Hashtable GetUserLinksTable(List<Link> links)
        {
            Hashtable userItemsTable = new Hashtable();
            foreach (Link l in links)
            {
                int key = l.From;
                if (userItemsTable.ContainsKey(key))
                {
                    List<Link> list = (List<Link>)userItemsTable[key];
                    list.Add(l);
                }
                else
                {
                    userItemsTable.Add(key, new List<Link>() { l });
                }
            }
            return userItemsTable;
        }

        /// <summary>
        /// Get reverse user id - links hash table, a.k.a. trustees.
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static Hashtable GetUserReverseLinksTable(List<Link> links)
        {
            Hashtable userItemsTable = new Hashtable();
            foreach (Link l in links)
            {
                int key = l.To;
                if (userItemsTable.ContainsKey(key))
                {
                    List<Link> list = (List<Link>)userItemsTable[key];
                    list.Add(l);
                }
                else
                {
                    userItemsTable.Add(key, new List<Link>() { l });
                }
            }
            return userItemsTable;
        }

        /// <summary>
        /// Check a social matrix is asymmetric?
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static bool IsAsymmetric(List<Link> links)
        {
            Console.WriteLine("links,{0}", links.Count);
            MyTable table = Tools.GetLinkTable(links);

            int counter = 0;
            foreach (int f in table.Keys)
            {
                Hashtable subTable = (Hashtable)table[f];
                foreach (int t in subTable.Keys)
                {
                    if (!table.ContainsKey(t, f))
                    {
                        // Console.WriteLine("{0},{1}", f, t);
                        counter++;
                    }
                }
            }
            Console.WriteLine("counter,{0}", counter);
            return counter == 0 ? true : false;
        }

        /// <summary>
        /// Get maximum user and item id, return in a tuple with two items.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns>maxUserId, maxItemId</returns>
        public static Tuple<int, int> GetMaxUserIdAndItemId(List<Rating> ratings)
        {
            int maxUserId = ratings.AsParallel().Max(r => r.UserId);
            int maxItemId = ratings.AsParallel().Max(r => r.ItemId);
            return Tuple.Create(maxUserId, maxItemId);
        }

        /// <summary>
        /// Get maximum score and minimum score in ratings.
        /// </summary>
        /// <param name="ratings"></param>
        /// <returns></returns>
        public static Tuple<double, double> GetMinAndMaxScore(List<Rating> ratings)
        {
            double maxScore = ratings.AsParallel().Max(r => r.Score);
            double minScore = ratings.AsParallel().Min(r => r.Score);
            return Tuple.Create(minScore, maxScore);
        }

        /// <summary>
        /// Get maximum node from link data, return in a tuple with two node ids.
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public static Tuple<int, int> GetMaxNodeId(List<Link> links)
        {
            int maxNodeId = links.AsParallel().Max(l => l.From);
            int maxNodeId2 = links.AsParallel().Max(l => l.To);
            return Tuple.Create(maxNodeId, maxNodeId2);
        }

        /// <summary>
        /// Tranform ratings to a matrix with given #rows and #columns.
        /// </summary>
        /// <param name="ratings"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static double[,] Transform(List<Rating> ratings, int rows, int columns)
        {
            // Matrix of user-item ratings, 0 filled when first constructed
            double[,] _matrix = new double[rows, columns];

            // Fill the matrix using list of ratings
            foreach (Rating r in ratings)
            {
                _matrix[r.UserId, r.ItemId] = r.Score;
            }
            return _matrix;
        }

        /// <summary>
        /// link.To += maximum of link.From + 1
        /// </summary>
        /// <param name="links">ratings read as List<Link>, where user id and item id in ratings are both 0-based.</param>
        /// <param name="maxFrom"></param>
        /// <param name="maxTo"></param>
        /// <returns>number of nodes.</returns>
        public static int TransformLinkedToId(List<Link> links, int maxFrom, int maxTo)
        {
            foreach (Link l in links)
            {
                l.To += (maxFrom + 1);
            }

            // Filling the reverse links in G
            int length = links.Count;
            for(int i = 0; i < length; i++)
            {
                links.Add(new Link(links[i].To, links[i].From, links[i].Weight));
            }

            return maxFrom + maxTo + 2;
        }

        /// <summary>
        /// split up ratings into train and test set with percentage of test size.
        /// </summary>
        /// <param name="ratings"></param>
        /// <param name="testSize"></param>
        /// <returns></returns>
        public static Tuple<List<Rating>, List<Rating>> TrainTestSplit(List<Rating> ratings, double testSize = 0.1)
        {
            if (ratings == null)
            {
                throw new ArgumentNullException();
            }

            Random random = new Random();
            List<Rating> baseRatings = new List<Rating>();
            List<Rating> testRatings = new List<Rating>();
            foreach(Rating r in ratings)
            {
                if (random.NextDouble() < testSize)
                {
                    testRatings.Add(r);
                }
                else
                {
                    baseRatings.Add(r);
                }

            }
            return Tuple.Create(baseRatings, testRatings);
        }

        /// <summary>
        /// split up ratings into train and test set with percentage of test size.
        /// </summary>
        /// <param name="recordTable">user id - item id - list of tags for this item</param>
        /// <param name="testSize"></param>
        /// <returns></returns>
        public static Tuple<MyTable, MyTable> TrainTestSplit(MyTable recordTable, double testSize = 0.1)
        {
            if (recordTable == null)
            {
                throw new ArgumentNullException();
            }

            Random random = new Random();
            MyTable baseRecordTable = new MyTable();
            MyTable testRecordTable = new MyTable();

            foreach (int userId in recordTable.Keys)
            {
                Hashtable subTable = (Hashtable)recordTable[userId];
                foreach (int itemId in subTable.Keys)
                {
                    var links = subTable[itemId];
                    if (random.NextDouble() < testSize)
                    {
                        testRecordTable.Add(userId, itemId, links);
                    }
                    else
                    {
                        baseRecordTable.Add(userId, itemId, links);
                    }
                }
            }
            return Tuple.Create(baseRecordTable, testRecordTable);
        }

        public static void WriteMatrix(double [,] matrix, string toFile, bool append = false, string encoding = "UTF-8")
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);
            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    writer.Write("{0},", matrix[i, j]);
                }
                writer.WriteLine("{0}", matrix[i, N - 1]);
            }
            writer.Close();
        }

        public static void WriteVector(double[] vector, string toFile, bool append = false, string encoding = "UTF-8")
        {
            int length = vector.Length;
            StreamWriter writer = new StreamWriter(toFile, append, Encoding.GetEncoding(encoding));
            for (int i = 0; i < length - 1; i++)
            {
                writer.Write("{0},", vector[i]);
            }
            writer.WriteLine("{0}", vector[length - 1]);
            writer.Close();
        }

        /// <summary>
        /// Randomly sample negative ratings for each user from his/her rated ratings. 
        /// Recommender systems in action, p84, vector xiang
        /// </summary>
        /// <param name="ratings">positive ratings</param>
        /// <param name="ratio">ratio = #(negative samples) / #(positive samples)</param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static List<Rating> RandomSelectNegativeSamples(List<Rating> ratings, int ratio = 1, bool verbose = false)
        {
            if (verbose)
            {
                Console.WriteLine("ratio,{0}", ratio);
            }

            List<Rating> positiveRatings = new List<Rating>();
            foreach (Rating r in ratings)
            {
                positiveRatings.Add(new Rating(r.UserId, r.ItemId, 1.0));
            }
            MyTable ratingTable = GetRatingTable(positiveRatings);
            int[] items = (int[])ratingTable.GetSubKeyList().ToArray(typeof(int));

            Random random = new Random();
            foreach (int uId in ratingTable.Keys)
            {
                Hashtable subTable = (Hashtable)ratingTable[uId];
                int counter = 0, ratedItems = subTable.Count;
                while ((counter < ratedItems * ratio) && (counter < items.Length - ratedItems))
                {
                    int iId = items[random.Next(items.Length)];
                    if (!subTable.ContainsKey(iId))
                    {
                        subTable.Add(iId, 0.0);   // negative samples
                        counter++;
                    }
                }
            }

            List<Rating> samples = new List<Rating>();
            foreach (int uId in ratingTable.Keys)
            {
                Hashtable subTable = (Hashtable)ratingTable[uId];
                foreach(int iId in subTable.Keys)
                {
                    double score = (double)subTable[iId];
                    samples.Add(new Rating(uId, iId, score));
                }
            }

            return samples;
        }

        /// <summary>
        /// Convert ratings to binary ratings
        /// </summary>
        /// <param name="ratings"></param>
        /// <param name="threshold">if score > threshold then return 1 else 0</param>
        /// <returns></returns>
        public static List<Rating> ConvertToBinary(List<Rating> ratings, double threshold = 0)
        {
            List<Rating> binaryRatings = new List<Rating>();
            foreach(Rating r in ratings)
            {
                binaryRatings.Add(new Rating(r.UserId, r.ItemId, r.Score > threshold ? 1.0 : 0.0));
            }
            return binaryRatings;
        }

        /// <summary>
        /// cui, see ICDM2009-p263-Hu&Koren
        /// </summary>
        /// <param name="rating"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static double WeightedRating(double r, double alpha = 40, string method = "linear")
        {
            double c = 0;
            if (method == "linear")
            {
                c = 1 + alpha * r;
            }
            else if (method == "log")
            {
                c = 1 + alpha * Math.Log(1 + r);
            }
            return c;
        }

        public static void UpdateConfidences(List<Rating> ratings, double alpha = 40, string method = "linear")
        {
            foreach(Rating r in ratings)
            {
                r.Confidence = WeightedRating(r.Score, alpha, method);
            }
        }

        /// <summary>
        /// user id - tuple(list of category one, list of category two).
        /// </summary>
        /// <param name="ratings">category one, positive ratings; category two, negative ratings.</param>
        /// <returns></returns>
        public static Hashtable GetClassifiedUserItemsTable(List<Rating> ratings)
        {
            Hashtable userItemsTable = new Hashtable();
            foreach (Rating r in ratings)
            {
                if (userItemsTable.ContainsKey(r.UserId))
                {
                    var tuple = (Tuple<List<Rating>, List<Rating>>)userItemsTable[r.UserId];
                    if (r.Score == 1)
                    {
                        tuple.Item1.Add(r);
                    }
                    else if(r.Score == 0)
                    {
                        tuple.Item2.Add(r);
                    }                    
                }
                else
                {
                    var tuple = Tuple.Create(new List<Rating>(), new List<Rating>());
                    if (r.Score == 1)
                    {
                        tuple.Item1.Add(r);
                    }
                    else if (r.Score == 0)
                    {
                        tuple.Item2.Add(r);
                    }
                    userItemsTable.Add(r.UserId, tuple);
                }
            }
            return userItemsTable;
        }

    }
}
