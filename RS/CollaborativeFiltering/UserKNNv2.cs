using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RS.DataType;
using RS.Data.Utility;
using RS.Evaluation;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// Top-N recommendation
    /// </summary>
    public class UserKNNv2
    {
        protected MyTable CalculateCooccurrences(Hashtable itemUsersTable, bool multithread = false)
        {
            MyTable cooccurrences = new MyTable();

            if (multithread)
            {
                int[] itemIds = new int[itemUsersTable.Keys.Count];
                itemUsersTable.Keys.CopyTo(itemIds, 0);
                Parallel.ForEach(itemIds, iId =>
                {
                    List<Rating> users = (List<Rating>)itemUsersTable[iId];
                    foreach (Rating u in users)
                    {
                        foreach (Rating v in users)
                        {
                            if (u.UserId == v.UserId)
                            {
                                continue;
                            }

                            lock(cooccurrences)
                            {
                                if (!cooccurrences.ContainsKey(u.UserId, v.UserId))
                                {
                                    cooccurrences.Add(u.UserId, v.UserId, 0.0);
                                }
                                cooccurrences[u.UserId, v.UserId] = (double)cooccurrences[u.UserId, v.UserId] + 1.0 / Math.Log(1 + users.Count);
                            }

                        }
                    }
                });

            }
            else
            {
                foreach (int iId in itemUsersTable.Keys)
                {
                    List<Rating> users = (List<Rating>)itemUsersTable[iId];
                    foreach (Rating u in users)
                    {
                        foreach (Rating v in users)
                        {
                            if (u.UserId == v.UserId)
                            {
                                continue;
                            }
                            if (!cooccurrences.ContainsKey(u.UserId, v.UserId))
                            {
                                cooccurrences.Add(u.UserId, v.UserId, 0.0);
                            }
                            cooccurrences[u.UserId, v.UserId] = (double)cooccurrences[u.UserId, v.UserId] + 1.0 / Math.Log(1 + users.Count);
                        }
                    }
                }
            } 
            return cooccurrences;
        }

        protected MyTable CalculateSimilarities(MyTable coourrencesTable, Hashtable userItemsTable)
        {
            MyTable wuv = new MyTable();
            foreach (int uId in coourrencesTable.Keys)
            {
                Hashtable subTable = (Hashtable)coourrencesTable[uId];
                List<Rating> uRatings = (List<Rating>)userItemsTable[uId];
                foreach (int vId in subTable.Keys)
                {
                    double coourrences = (double)subTable[vId];                    
                    List<Rating> vRatings = (List<Rating>)userItemsTable[vId];
                    wuv.Add(uId, vId, coourrences / Math.Sqrt(uRatings.Count * vRatings.Count));
                }
            }
            return wuv;
        }

        protected List<Link> GetSimilarUsers(MyTable W, int userId, int K = 80)
        {
            List<Link> weights = new List<Link>();
            Hashtable subTable = (Hashtable)W[userId];
            foreach (int vId in subTable.Keys)
            {
                double _w = (double)subTable[vId];
                Link l = new Link(userId, vId, _w);
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
                List<Link> similarUsers = GetSimilarUsers(W, userId, K);                
                foreach (Link l in similarUsers)
                {
                    int vId = l.To;     // similar user v
                    Hashtable Nv = (Hashtable)ratingTable[vId];    // ratings of user v
                    foreach (int iId in Nv.Keys)
                    {
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
            foreach(int uId in recommendedTable.Keys)
            {
                List<Rating> li = new List<Rating>();
                Hashtable subTable = (Hashtable)recommendedTable[uId];
                foreach(int iId in subTable.Keys)
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

            MyTable coourrrenceTable = CalculateCooccurrences(itemUsersTable, true);
            MyTable wuv = CalculateSimilarities(coourrrenceTable, userItemsTable);

            MyTable ratingTable = Tools.GetRatingTable(train);

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity");   
            List<Rating> recommendations = GetRecommendations(ratingTable, wuv, K, N);
            var pr = Metrics.PrecisionAndRecall(recommendations, test);
            var cp = Metrics.CoverageAndPopularity(recommendations, train); // note: train ratings
            Console.WriteLine("{0},{1},{2},{3},{4}", N, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
        }

        public void TryTopN(List<Rating> train, List<Rating> test)
        {
            Hashtable userItemsTable = Tools.GetUserItemsTable(train);
            Hashtable itemUsersTable = Tools.GetItemUsersTable(train);

            MyTable coocurrenceTable = CalculateCooccurrences(itemUsersTable, true);
            MyTable wuv = CalculateSimilarities(coocurrenceTable, userItemsTable);
            MyTable ratingTable = Tools.GetRatingTable(train);

            List<int> Ks = new List<int>() { 5, 10, 20, 40, 80, 160 };
            List<int> Ns = new List<int>() { 1, 5, 10, 15, 20, 25, 30 };

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity");
            foreach (int k in Ks)
            {
                Console.Write(k);
                foreach(int n in Ns)
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
