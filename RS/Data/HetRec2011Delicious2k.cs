using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.DataType;
using RS.Data.Utility;
using RS.CollaborativeFiltering;
using RS.ContentBasedFiltering;

namespace RS.Data
{
    /// <summary>
    /// To download: https://grouplens.org/datasets/hetrec-2011/
    /// </summary>
    public static class HetRec2011Delicious2k
    {
        public static string DefalultDirectory = @"D:\data\dataset_dm\hetrec2011-delicious-2k\";

        public static string DefaultRecordFile = DefalultDirectory + @"user_taggedbookmarks-timestamps.dat";

        public static string DefaultUserContactFile = DefalultDirectory + @"user_contacts-timestamps.dat";  // user side, a social network
        public static string DefaultBookmarkFile    = DefalultDirectory + @"bookmarks.dat";  // bookmark side, web contents
        public static string DefaultTagFile         = DefalultDirectory + @"tags.dat";

        public static string BaseUserItemTagFile = DefalultDirectory + @"item_recommendation.base";
        public static string TestUserItemTagFile = DefalultDirectory + @"item_recommendation.test";

        public static int MaxUserId = 943;
        public static int MaxItemId = 1682;
        public static int MaxTagId  = 1682;


        /// <summary>
        /// A record: userID	bookmarkID	tagID	timestamp
        /// </summary>
        /// <param name="file">user_taggedbookmarks-timestamps.dat</param>
        /// <param name="separator"></param>
        /// <returns>MyTable: user Id - item Id - List<Link>, where a link is represented as item Id - tag Id </returns>
        public static MyTable GetRecords(string file, string separator = "\t")
        {
            if (!File.Exists(file))
            {
                throw new ArgumentException("File doesn't exist: " + file);
            }

            StreamReader reader = new StreamReader(file);
            MyTable recordsTable = new MyTable();
            string firstLine = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] elements = line.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                
                if (elements.Length == 4)
                {
                    int userId = Int32.Parse(elements[0]);
                    int itemId = Int32.Parse(elements[1]);
                    int tagId  = Int32.Parse(elements[2]);
                    string timestamp = elements[3];

                    Link itemTag = new Link(itemId, tagId);

                    if (recordsTable.ContainsKey(userId, itemId))
                    {
                        List<Link> links = (List<Link>)recordsTable[userId, itemId];
                        links.Add(itemTag);
                    }
                    else
                    {
                        List<Link> links = new List<Link>() { itemTag };
                        recordsTable.Add(userId, itemId, links);
                    }
                }
            }
            reader.Close();
            return recordsTable;
        }


        /// <summary>
        /// Get relation tables
        /// 1. user - item - #tags
        /// 2. user - item - #tags
        /// 3. item - tag - #tag
        /// </summary>
        /// <param name="recordTable"></param>
        /// <returns></returns>
        public static Tuple<List<Rating>, List<Link>, List<Link>> GetRelations(MyTable recordTable)
        {
            List<Rating> userItemCount = new List<Rating>(); // user - item - #tag
            List<Link> userTagCount = new List<Link>();      // user - tag - #tag
            List<Link> itemTagCount = new List<Link>();      // item - tag - #tag

            MyTable userTagTable = new MyTable();
            MyTable itemTagTable = new MyTable();

            foreach (int userId in recordTable.Keys)
            {
                Hashtable subTable = (Hashtable)recordTable[userId];
                foreach(int itemId in subTable.Keys)
                {
                    List<Link> links = (List<Link>)subTable[itemId];
                    Rating rating = new Rating(userId, itemId, links.Count);
                    userItemCount.Add(rating);

                    foreach(Link l in links)
                    {
                        // user - tag - # tags table
                        //int itemId = l.From;
                        int tagId = l.To;

                        if (userTagTable.ContainsKey(userId, tagId))
                        {
                            userTagTable[userId, tagId] = (int)userTagTable[userId, tagId] + 1;
                        }
                        else
                        {
                            userTagTable.Add(userId, tagId, 1);
                        }

                        // item - tag - #tags table
                        if (itemTagTable.ContainsKey(itemId, tagId))
                        {
                            itemTagTable[itemId, tagId] = (int)itemTagTable[itemId, tagId] + 1;
                        }
                        else
                        {
                            itemTagTable.Add(itemId, tagId, 1);
                        }
                    }
                }
            }

            foreach (int userId in userTagTable.Keys)
            {
                Hashtable subTable = (Hashtable)userTagTable[userId];
                foreach (int tagId in subTable.Keys)
                {
                    int counts = (int)subTable[tagId];
                    Link l = new Link(userId, tagId, counts);
                    userTagCount.Add(l);
                }
            }

            // cull up for item - tag 
            foreach (int itemId in itemTagTable.Keys)
            {
                Hashtable subTable = (Hashtable)itemTagTable[itemId];
                foreach (int tagId in subTable.Keys)
                {
                    int counts = (int)subTable[tagId];
                    Link l = new Link(itemId, tagId, counts);
                    itemTagCount.Add(l);
                }
            }
            return Tuple.Create(userItemCount, userTagCount, itemTagCount);
        }



        public static void Test()
        {
            MyTable table =  GetRecords(DefaultRecordFile);
            Console.WriteLine(table.Keys.Count);
            Console.WriteLine(table.SubKeyTable.Count);

            var v = GetRelations(table);
            Console.WriteLine(v.Item1.Count);
            Console.WriteLine(v.Item2.Count);
            Console.WriteLine(v.Item3.Count);
        }

        public static void UserKNNv2Test(double testSize = 0.1)
        {
            MyTable table = GetRecords(DefaultRecordFile);
            var v = GetRelations(table);
            List<Rating> ratings = v.Item1;
            var data = Tools.TrainTestSplit(ratings, testSize);
            UserKNNv2 knn = new UserKNNv2();
            knn.TryTopN(data.Item1, data.Item2);
        }

        public static void TagBasedItemKNNTest(double testSize = 0.1)
        {
            MyTable recordTable = GetRecords(DefaultRecordFile);
            var tables = Tools.TrainTestSplit(recordTable, testSize);
            var v1 = GetRelations(tables.Item1);
            var v2 = GetRelations(tables.Item2);

            TagBasedItemKNN knn = new TagBasedItemKNN();
            knn.TryTopN(v1.Item1, v2.Item1, v1.Item2, 80, 10);
        }


    }
}
