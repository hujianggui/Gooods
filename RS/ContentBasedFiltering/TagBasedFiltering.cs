using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.DataType;
using RS.Data.Utility;
using RS.Evaluation;

namespace RS.ContentBasedFiltering
{
    /// <summary>
    /// 推荐系统实践，p123, xiang
    /// </summary>
    public class TagBasedFiltering
    {
        protected void PrintParameters(List<Rating> train, List<Rating> test, List<Link> userTags, List<Link> itemTags)
        {
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("userTags,{0}", userTags.Count);
            Console.WriteLine("itemTags,{0}", itemTags.Count);
        }

        protected MyTable GetRecommendations(MyTable ratingTable, MyTable userTagTable, MyTable tagItemTable)
        {
            MyTable recommendTable = new MyTable();

            int[] userIds = new int[ratingTable.Keys.Count];
            ratingTable.Keys.CopyTo(userIds, 0);
            Parallel.ForEach(userIds, userId =>
            {
                Hashtable subTable = (Hashtable)ratingTable[userId];
                if (userTagTable.ContainsMainKey(userId))
                {               
                    Hashtable tagTable = (Hashtable)userTagTable[userId];
                    foreach (int tagId in tagTable.Keys)
                    {
                        if (!tagItemTable.ContainsMainKey(tagId))
                        {
                            continue;
                        }
                        Hashtable itemTable = (Hashtable)tagItemTable[tagId];
                        foreach (int itemId in itemTable.Keys)
                        {
                            // if user has rated this item
                            if (subTable.ContainsKey(itemId))
                            {
                                continue;
                            }

                            double wut = (double)tagTable[tagId];
                            double wti = (double)itemTable[itemId];
                            double p = wut * wti;

                            lock(recommendTable)
                            {
                                if (recommendTable.ContainsKey(userId, itemId))
                                {
                                    recommendTable[userId, itemId] = (double)recommendTable[userId, itemId] + p;
                                }
                                else
                                {
                                    recommendTable.Add(userId, itemId, p);
                                }
                            }
                        }
                    }
                }
            });

            return recommendTable;
        }

        protected MyTable GetRecommendationsByTFIDF(MyTable ratingTable, MyTable userTagTable, Hashtable tagUsersTable, MyTable tagItemTable)
        {
            MyTable recommendTable = new MyTable();

            int[] userIds = new int[ratingTable.Keys.Count];
            ratingTable.Keys.CopyTo(userIds, 0);
            Parallel.ForEach(userIds, userId =>
            {
                Hashtable subTable = (Hashtable)ratingTable[userId];
                if (userTagTable.ContainsMainKey(userId))
                {
                    Hashtable tagTable = (Hashtable)userTagTable[userId];
                    foreach (int tagId in tagTable.Keys)
                    {
                        if (!tagItemTable.ContainsMainKey(tagId))
                        {
                            continue;
                        }
                        Hashtable itemTable = (Hashtable)tagItemTable[tagId];
                        foreach (int itemId in itemTable.Keys)
                        {
                            // if user has rated this item
                            if (subTable.ContainsKey(itemId))
                            {
                                continue;
                            }

                            List<Link> n_b = (List<Link>)tagUsersTable[tagId];  // # of users who used this tag
                            double wut = (double)tagTable[tagId];
                            double wti = (double)itemTable[itemId];
                            double p = wut / Math.Log(1 + n_b.Count) * wti;

                            lock (recommendTable)
                            {
                                if (recommendTable.ContainsKey(userId, itemId))
                                {
                                    recommendTable[userId, itemId] = (double)recommendTable[userId, itemId] + p;
                                }
                                else
                                {
                                    recommendTable.Add(userId, itemId, p);
                                }
                            }
                        }
                    }
                }
            });

            return recommendTable;
        }

        protected MyTable GetRecommendationsByTFIDFPlusPlus(MyTable ratingTable, MyTable userTagTable, 
            Hashtable tagUsersTable, MyTable tagItemTable, Hashtable itemTagsTable)
        {
            MyTable recommendTable = new MyTable();

            int[] userIds = new int[ratingTable.Keys.Count];
            ratingTable.Keys.CopyTo(userIds, 0);
            Parallel.ForEach(userIds, userId =>
            {
                Hashtable subTable = (Hashtable)ratingTable[userId];
                if (userTagTable.ContainsMainKey(userId))
                {
                    Hashtable tagTable = (Hashtable)userTagTable[userId];
                    foreach (int tagId in tagTable.Keys)
                    {
                        if (!tagItemTable.ContainsMainKey(tagId))
                        {
                            continue;
                        }
                        Hashtable itemTable = (Hashtable)tagItemTable[tagId];
                        foreach (int itemId in itemTable.Keys)
                        {
                            // if user has rated this item
                            if (subTable.ContainsKey(itemId))
                            {
                                continue;
                            }

                            List<Link> n_b = (List<Link>)tagUsersTable[tagId];  // # of users who used this tag
                            List<Link> n_i = (List<Link>)itemTagsTable[itemId];  // # of users who used this tag
                            double wut = (double)tagTable[tagId];
                            double wti = (double)itemTable[itemId];
                            double pui = wut / Math.Log(1 + n_b.Count) * wti / Math.Log(1 + n_i.Count);

                            lock (recommendTable)
                            {
                                if (recommendTable.ContainsKey(userId, itemId))
                                {
                                    recommendTable[userId, itemId] = (double)recommendTable[userId, itemId] + pui;
                                }
                                else
                                {
                                    recommendTable.Add(userId, itemId, pui);
                                }
                            }
                        }
                    }
                }
            });

            return recommendTable;
        }

        protected List<Rating> GetSortedRatings(MyTable recommendTable, int reservedMaximumN = 30)
        {
            List<Rating> ratings = new List<Rating>();
            foreach (int userId in recommendTable.Keys)
            {
                Hashtable subTable = (Hashtable)recommendTable[userId];
                List<Rating> list = new List<Rating>();
                foreach (int itemId in subTable.Keys)
                {
                    Rating r = new Rating(userId, itemId, (double)subTable[itemId]);
                    list.Add(r);
                }
                List<Rating> sortedList = list.OrderByDescending(l => l.Score).ToList();
                ratings.AddRange(sortedList.GetRange(0, Math.Min(sortedList.Count, reservedMaximumN)));
            }
            return ratings;
        }

        public void TrySimpleTagBased(List<Rating> train, List<Rating> test, List<Link> userTags, List<Link> itemTags)
        {
            PrintParameters(train, test, userTags, itemTags);

            MyTable userItemTable = Tools.GetRatingTable(train);
            MyTable userTagTable = Tools.GetLinkTable(userTags);
            MyTable tagItemTable = Tools.GetReversedLinkTable(itemTags);
            Hashtable tagUsersTable = Tools.GetUserReverseLinksTable(userTags);
            Hashtable itemTagesTable = Tools.GetUserLinksTable(itemTags);

            //MyTable recommendTable = GetRecommendations(userItemTable, userTagTable, tagItemTable);
            //MyTable recommendTable = GetRecommendationsByTFIDF(userItemTable, userTagTable, tagUsersTable, tagItemTable);
            MyTable recommendTable = GetRecommendationsByTFIDFPlusPlus(userItemTable, userTagTable, tagUsersTable, tagItemTable, itemTagesTable);

            List<Rating> recommendedRatings = GetSortedRatings(recommendTable, 30);

            List<int> Ns = new List<int>() { 1, 5, 10, 15, 20, 25, 30 };

            Console.WriteLine("N,P,R,Coverage,Popularity");
            
            foreach (int n in Ns)
            {
                List<Rating> recommendations = Tools.GetSubset(recommendedRatings, n);
                var pr = Metrics.PrecisionAndRecall(recommendations, test);
                var cp = Metrics.CoverageAndPopularity(recommendations, train);
                Console.WriteLine("{0},{1},{2},{3},{4}", n, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
            }          
        }
    }
}   
