using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.ContentBasedFiltering
{
    /// <summary>
    /// https://dl.acm.org/citation.cfm?doid=1240624.1240772
    /// CHI 2007
    /// </summary>
    public class TagBasedItemKNN
    {
        protected MyTable CalculateCooccurrences(Hashtable tagItemsTable)
        {
            MyTable cooccurrences = new MyTable();           
            int[] tagIds = new int[tagItemsTable.Keys.Count];
            tagItemsTable.Keys.CopyTo(tagIds, 0);

            Parallel.ForEach(tagIds, tId =>
            {
                List<Link> items = (List<Link>)tagItemsTable[tId];
                foreach (Link u in items)
                {
                    foreach (Link v in items)
                    {
                        if (u.From == v.From)
                        {
                            continue;
                        }

                        lock (cooccurrences)
                        {
                            if (!cooccurrences.ContainsKey(u.From, v.From))
                            {
                                cooccurrences.Add(u.From, v.From, 0.0);
                            }
                            cooccurrences[u.From, v.From] = (double)cooccurrences[u.From, v.From]; // + 1.0 / Math.Log(1 + items.Count);
                        }

                    }
                }
            });

            return cooccurrences;
        }

        protected MyTable CalculateSimilarities(MyTable coourrencesTable, Hashtable itemTagsTable)
        {
            MyTable wuv = new MyTable();
            foreach (int uId in coourrencesTable.Keys)
            {
                Hashtable subTable = (Hashtable)coourrencesTable[uId];
                List<Link> uRatings = (List<Link>)itemTagsTable[uId];
                foreach (int vId in subTable.Keys)
                {
                    double coourrences = (double)subTable[vId];
                    List<Link> vRatings = (List<Link>)itemTagsTable[vId];
                    wuv.Add(uId, vId, coourrences / Math.Sqrt(uRatings.Count * vRatings.Count));
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

        public Hashtable GetSimilarItems(MyTable W, int K = 80)
        {
            Hashtable similarItems = new Hashtable();
            foreach (int itemId in W.Keys)
            {
                List<Link> selectedItems = GetSimilarItems(W, itemId, K);
                similarItems.Add(itemId, selectedItems);
            }
            return similarItems;
        }

        public List<Rating> GetRecommendations(MyTable ratingTable, MyTable W, int K = 80, int N = 10)
        {
            MyTable recommendedTable = new MyTable();
            Hashtable similarItemsTable = GetSimilarItems(W, K);
            foreach (int userId in ratingTable.Keys)
            {
                Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                foreach (int itemId in Nu.Keys)
                {
                    if (!similarItemsTable.ContainsKey(itemId))
                    {
                        continue;
                    }

                    List<Link> similarItems = (List<Link>)similarItemsTable[itemId];    // 优化
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


        public void TryTopN(List<Rating> train, List<Rating> test, List<Link> itemTags, int K, int N = 10)
        {
            Hashtable itemTagsTable = Tools.GetUserLinksTable(itemTags);
            Hashtable tagItemsTable = Tools.GetUserReverseLinksTable(itemTags);

            MyTable coocurrenceTable = CalculateCooccurrences(tagItemsTable);
            MyTable wij = CalculateSimilarities(coocurrenceTable, itemTagsTable);

            MyTable ratingTable = Tools.GetRatingTable(train);

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity");
            List<Rating> recommendations = GetRecommendations(ratingTable, wij, K, N);

            var pr = Metrics.PrecisionAndRecall(recommendations, test);
            var cp = Metrics.CoverageAndPopularity(recommendations, train); 
            Console.WriteLine("{0},{1},{2},{3},{4}", K, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
        }
    }
}
