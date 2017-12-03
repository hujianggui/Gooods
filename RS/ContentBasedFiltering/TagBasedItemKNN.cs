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
        protected double CosineSimilarity(Hashtable tags1, Hashtable tags2)
        {
            double nominator = 0.0;
            double denominator1 = 0.0;
            double denominator2 = 0.0;

            foreach (int tagId in tags1.Keys)
            {
                if (tags2.ContainsKey(tagId))
                {
                    nominator += (double)tags1[tagId] * (double)tags2[tagId];
                }
                denominator1 += (double)tags1[tagId] * (double)tags1[tagId];
            }
            if (nominator == 0)
            {
                return 0.0;
            }
            foreach (int tagId in tags2.Keys)
            {
                denominator2 += (double)tags2[tagId] * (double)tags2[tagId];
            }
            return nominator / Math.Sqrt(denominator1 * denominator2);
        }

        protected Hashtable CalculateSimilarItems(MyTable itemTagTable, int reservedMaximumK = 160)
        {
            int[] itemIds = new int[itemTagTable.Keys.Count];
            itemTagTable.Keys.CopyTo(itemIds, 0);
            Hashtable similarItemsTable = new Hashtable();

            Parallel.ForEach(itemIds, itemId1 =>
            {
                Hashtable tags1 = (Hashtable)itemTagTable[itemId1];
                List<Link> similarItems = new List<Link>();
                foreach (int itemId2 in itemTagTable.Keys)
                {
                    if (itemId1 == itemId2)
                    {
                        continue;
                    }

                    Hashtable tags2 = (Hashtable)itemTagTable[itemId2];
                    double s = CosineSimilarity(tags1, tags2);
                    Link link = new Link(itemId1, itemId2, s);
                    similarItems.Add(link);
                }
                List<Link> sortedSimilarItems = similarItems.OrderByDescending(l => l.Weight).ToList();
                List<Link> selectedItems = sortedSimilarItems.GetRange(0, Math.Min(sortedSimilarItems.Count, reservedMaximumK));
                lock (similarItemsTable)
                {
                    similarItemsTable.Add(itemId1, selectedItems);
                    //Tools.WriteLinks(selectedItems, @"D:\items.csv");
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("{0:f6}", similarItemsTable.Count * 1.0 / itemTagTable.Count);
                }
            });

            //foreach(int itemId1 in itemTagTable.Keys)
            //{
            //    Hashtable tags1 = (Hashtable)itemTagTable[itemId1];
            //    List<Link> similarItems = new List<Link>();
            //    foreach (int itemId2 in itemTagTable.Keys)
            //    {
            //        if (itemId1 == itemId2)
            //        {
            //            continue;
            //        }

            //        Hashtable tags2 = (Hashtable)itemTagTable[itemId2];
            //        double s = CosineSimilarity(tags1, tags2);
            //        Link link = new Link(itemId1, itemId2, s);
            //        similarItems.Add(link);
            //    }
            //    List<Link> sortedSimilarItems = similarItems.OrderByDescending(l => l.Weight).ToList();
            //    List<Link> selectedItems = sortedSimilarItems.GetRange(0, Math.Min(sortedSimilarItems.Count, reservedMaximumK));
            //    similarItemsTable.Add(itemId1, selectedItems);  
            //}            

            return similarItemsTable;
        }


        protected Hashtable GetSimilarItems(Hashtable similarItemTable, int K = 5)
        {
            Hashtable selectedItemTable = new Hashtable();
            foreach (int itemId in similarItemTable)
            {
                List<Link> links = (List<Link>)similarItemTable[itemId];
                selectedItemTable.Add(itemId, links.GetRange(0, Math.Min(K, links.Count)));
            }
            return selectedItemTable;
        }

        public List<Rating> GetRecommendations(MyTable ratingTable, Hashtable similarItemsTable, int K = 80, int N = 10)
        {
            MyTable recommendedTable = new MyTable();
            foreach (int userId in ratingTable.Keys)
            {
                Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                foreach (int itemId in Nu.Keys)
                {
                    if (!similarItemsTable.ContainsKey(itemId))
                    {
                        continue;
                    }

                    List<Link> similarItems = (List<Link>)similarItemsTable[itemId];    
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
            MyTable itemTagTable = Tools.GetLinkTable(itemTags);
            Hashtable similarItemsTable = CalculateSimilarItems(itemTagTable, 160);

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity");
            MyTable ratingTable = Tools.GetRatingTable(train); 
            List<Rating> recommendations = GetRecommendations(ratingTable, similarItemsTable, K, N);            
            var pr = Metrics.PrecisionAndRecall(recommendations, test);
            var cp = Metrics.CoverageAndPopularity(recommendations, train); 
            Console.WriteLine("{0},{1},{2},{3},{4}", K, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
        }

        public void TryTopN(List<Rating> train, List<Rating> test, List<Link> itemTags)
        {
            MyTable itemTagTable = Tools.GetLinkTable(itemTags);
            Hashtable similarItemsTable = CalculateSimilarItems(itemTagTable, 160);

            MyTable ratingTable = Tools.GetRatingTable(train);

            List<int> Ks = new List<int>() { 5, 10, 20, 40, 80, 160 };
            List<int> Ns = new List<int>() { 1, 5, 10, 15, 20, 25, 30 };

            Console.WriteLine("K(Cosine),N,P,R,Coverage,Popularity");
            foreach (int k in Ks)
            {
                Console.Write(k);
                foreach (int n in Ns)
                {
                    List<Rating> recommendations = GetRecommendations(ratingTable, similarItemsTable, k, n);
                    var pr = Metrics.PrecisionAndRecall(recommendations, test);
                    var cp = Metrics.CoverageAndPopularity(recommendations, train);
                    Console.WriteLine(",{0},{1},{2},{3},{4}", n, pr.Item1, pr.Item2, cp.Item1, cp.Item2);
                }
            }
        }
    }
}
