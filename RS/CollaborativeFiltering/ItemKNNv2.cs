using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;
using RS.Data.Utility;
using RS.Evaluation;

namespace RS.CollaborativeFiltering
{
    public class ItemKNNv2
    {
        protected MyTable CalculateCooccurrences(Hashtable userItemsTable)
        {
            MyTable cooccurrences = new MyTable();
            foreach (int uId in userItemsTable.Keys)
            {
                List<Rating> items = (List<Rating>)userItemsTable[uId];
                foreach (Rating i in items)
                {
                    foreach (Rating j in items)
                    {
                        if (i.ItemId == j.ItemId)
                        {
                            continue;
                        }
                        if (!cooccurrences.ContainsKey(i.ItemId, j.ItemId))
                        {
                            cooccurrences.Add(i.ItemId, j.ItemId, 0.0);
                        }
                        cooccurrences[i.ItemId, j.ItemId] = (double)cooccurrences[i.ItemId, j.ItemId] + 1.0;
                    }
                }
            }
            return cooccurrences;
        }

        protected MyTable CalculateSimilarities(MyTable coourrencesTable, Hashtable itemUsersTable)
        {
            MyTable wuv = new MyTable();
            foreach (int iId in coourrencesTable.Keys)
            {
                Hashtable subTable = (Hashtable)coourrencesTable[iId];
                List<Rating> iRatings = (List<Rating>)itemUsersTable[iId];
                foreach (int jId in subTable.Keys)
                {
                    double coourrences = (double)subTable[jId];
                    List<Rating> jRatings = (List<Rating>)itemUsersTable[jId];
                    wuv.Add(iId, jId, coourrences * 1.0 / Math.Sqrt(iRatings.Count + jRatings.Count));
                }
            }
            return wuv;
        }

        protected List<Link> GetSimilarItems(MyTable W, int itemId, int K = 80)
        {
            List<Link> weights = new List<Link>();
            Hashtable subTable = (Hashtable)W[itemId];
            foreach (int vId in subTable.Keys)
            {
                double _w = (double)subTable[vId];
                Link l = new Link(itemId, vId, _w);
                weights.Add(l);
            }
            List<Link> sortedWeights = weights.OrderByDescending(l => l.Weight).ToList();
            return sortedWeights.GetRange(0, Math.Min(weights.Count, K));
        }

        protected List<Rating> GetRecommendations(MyTable ratingTable, MyTable W, int K = 80, int N = 10)
        {
            MyTable recommendedTable = new MyTable();
            foreach (int userId in ratingTable.Keys)
            {
                Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                foreach (int itemId in Nu.Keys)
                {
                    List<Link> similarItems = GetSimilarItems(W, itemId, K);
                    foreach (Link l in similarItems)
                    {
                        int iId = l.To;
                        if (Nu.ContainsKey(iId))
                        {
                            continue;
                        }

                        if (recommendedTable.ContainsKey(userId, iId))
                        {
                            double _t = (double)recommendedTable[userId, iId];
                            recommendedTable[userId, iId] = _t + l.Weight;
                        }
                        else
                        {
                            recommendedTable.Add(userId, iId, l.Weight);
                        }
                    }

                }
            }

            List<Rating> recommendedItems = new List<Rating>();
            foreach (int uId in recommendedTable.Keys)
            {
                List<Rating> li = new List<Rating>();
                Hashtable subTable = (Hashtable)recommendedTable[uId];
                foreach (int iId in subTable.Keys)
                {
                    double _t = (double)subTable[iId];
                    li.Add(new Rating(uId, iId, _t));
                }
                List<Rating> sortedLi = li.OrderByDescending(r => r.Score).ToList();
                recommendedItems.AddRange(sortedLi.GetRange(0, Math.Min(sortedLi.Count, N)));
            }
            return recommendedItems;
        }

        public void TryTopN(List<Rating> train, List<Rating> test, int K, int N = 10)
        {
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable itemUsersTable = Tools.GetItemUsersTable(train);

            MyTable coourrrenceTable = CalculateCooccurrences(userItemsTable);
            MyTable wuv = CalculateSimilarities(coourrrenceTable, itemUsersTable);

            MyTable ratingTable = Tools.GetRatingTable(train);

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity"); 
            List<Rating> recommendations = GetRecommendations(ratingTable, wuv, K, N);
            var pr = Metrics.PrecisionAndRecall(recommendations, test);
            var cp = Metrics.CoverageAndPopularity(recommendations, train);
            Console.WriteLine("{0},{1},{2},{3},{4}", K, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
        }

        public void TryTopN(List<Rating> train, List<Rating> test)
        {
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable itemUsersTable = Tools.GetItemUsersTable(train);
            MyTable coourrrenceTable = CalculateCooccurrences(userItemsTable);
            MyTable wuv = CalculateSimilarities(coourrrenceTable, itemUsersTable);
            MyTable ratingTable = Tools.GetRatingTable(train);

            List<int> Ks = new List<int>() { 5, 10, 20, 40, 80, 160 };
            List<int> Ns = new List<int>() { 1, 5, 10, 15, 20, 25, 30 };

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity");
            foreach (int k in Ks)
            {
                Console.Write(k);
                foreach (int n in Ns)
                {
                    List<Rating> recommendations = GetRecommendations(ratingTable, wuv, k, n);
                    var pr = Metrics.PrecisionAndRecall(recommendations, test);
                    var cp = Metrics.CoverageAndPopularity(recommendations, train);
                    Console.WriteLine(",{0},{1},{2},{3},{4}", n, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
                }
            }            
        }
    }
}
