using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;
using RS.Evaluation;

namespace RS.Algorithm
{

    /// <summary>
    /// class Node
    /// </summary>
    public class Node
    {
        public int Id { get; private set; }
        public double Weight { get; set; }

        public int InDegree { get; set; }   // Number of in links

        public int OutDegree { get; set; }  // Number of out links

        // Constructor
        public Node(int id)
        {
            this.Id = id;
        }

        public Node(int id, double weight)
        {
            this.Id = id;
            this.Weight = weight;
        }

    }


    /// <summary>
    /// class PageRank
    /// url: http://blog.jobbole.com/71431/
    /// http://zh.wikipedia.org/wiki/PageRank
    /// </summary>
    public class PageRank
    {
        protected List<Link> Edges { get; set; }

        protected List<Node> Nodes { get; set; }

        protected int NumberOfNodes { get; set; }

        protected double[] PR { get; set; } // 1-based, result

        protected double[] P0 { get; set; } // 1-based, initial value


        public int MaxEpoch = 10;

        public double Convergency = 10e-6;

        public double Alpha = 0.8;

        // Constructor functions

        public PageRank(List<Link> edges, List<Node> nodes)
        {
            this.Edges = edges;
            this.Nodes = nodes;
        }

        public PageRank(List<Link> edges, int nodes)
        {
            this.Edges = edges;
            this.NumberOfNodes = nodes;
        }

        // Memeber functions

        /// <summary>
        /// Update weight of an edge using out-links (or out-degree)
        /// Have a try on Linq.
        /// </summary>
        private void Initial()
        {
            if (this.Edges == null)
            {
                throw new ArgumentNullException();
            }

            //var outLinkCount = (from e in Edges group e by e.From into g
            //                select new {g.Key, NumOutLinks = g.Count()});

            //foreach (Edge e in this.Edges)
            //{
            //    e.Weight = 1.0 / (outLinkCount.SingleOrDefault( g => g.Key == e.From).NumOutLinks);
            //}

            Hashtable outLinkTable = new Hashtable();
            foreach (Link e in this.Edges)
            {
                if (!outLinkTable.ContainsKey(e.From))
                {
                    outLinkTable.Add(e.From, new List<int>() { e.To });
                }
                else
                {
                    List<int> outlinks = (List<int>)outLinkTable[e.From];
                    outlinks.Add(e.To);
                }
            }
            foreach (Link e in this.Edges)
            {
                List<int> outlinks = (List<int>)outLinkTable[e.From];
                e.Weight = 1.0 / outlinks.Count;
            }

            PR = new double[NumberOfNodes + 1];
            P0 = new double[NumberOfNodes + 1];
            foreach(int f in outLinkTable.Keys)
            {
                P0[f] = PR[f] = 1.0 / outLinkTable.Count;
            }
        }

        private double SumOfSquareDifference(double[] array1, double[] array2)
        {
            double sum = 0.0;
            int length = array1.Length;
            for (int i = 0; i < length; i++)
            {
                double error = array1[i] - array2[i];
                sum += (error * error);
            }
            return sum;
        }

        protected double[] Epoch(Hashtable inLinkTable, double[] rank)
        {
            double[] result = new double[rank.Length];
            foreach (int id in inLinkTable.Keys)
            {
                List<Link> inLinks = (List<Link>)inLinkTable[id];
                double sum = 0.0;
                foreach (Link e in inLinks)
                {
                    sum += (rank[e.From] * e.Weight);
                }
                result[id] = sum * Alpha + (1 - Alpha) * P0[id];
            }
            return result;
        }

        public void TryRanking()
        {
            Initial();

            Hashtable inLinkTable = new Hashtable();
            foreach (Link e in this.Edges)
            {
                if (!inLinkTable.ContainsKey(e.To))
                {
                    inLinkTable.Add(e.To, new List<Link>() { e });
                }
                else
                {
                    List<Link> outlinks = (List<Link>)inLinkTable[e.To];
                    outlinks.Add(e);
                }
            }

            PR = Epoch(inLinkTable, P0);
            double difference = SumOfSquareDifference(PR, P0);
            Console.WriteLine("Epoch, {0}, Difference, {1}", 1, difference);

            for (int epoch = 2; epoch < MaxEpoch && difference > Convergency; epoch++)
            {
                double[] ranking = Epoch(inLinkTable, PR);
                difference = SumOfSquareDifference(PR, ranking);
                Console.WriteLine("Epoch, {0}, Difference, {1}", epoch, difference);
                PR = ranking;
            }

            // Get item part, node id as item id
            List<Node> nodes = new List<Node>();
            for (int i = 1; i < this.NumberOfNodes + 1; i++)
            {
                nodes.Add(new Node(i, PR[i]));
            }

            // Sort, descending
            var rankNodes = nodes.OrderByDescending(n => n.Weight);

            this.Nodes = rankNodes.ToList();
        }

        public double[] GetPR()
        {
            return this.PR;
        }

        // Design for movielens
        public void TryListRecommendation(List<Rating> baseRatings, List<Rating> testRatings, int userNumber, int itemNumber, int k)
        {
            // Get item part, node id as item id
            List<Node> nodes = new List<Node>();
            for (int i = userNumber + 1; i < userNumber + itemNumber + 1; i++)
            {
                nodes.Add(new Node(i - userNumber, PR[i]));
            }

            // Sort, descending
            var rankedItems = nodes.OrderByDescending(n => n.Weight);

            // Get u not rate item ralate value from Pk
            MyTable table = new MyTable();
            foreach (Rating r in baseRatings) 
            {
                if (!table.ContainsKey(r.UserId, r.ItemId))
                {
                    table.Add(r.UserId, r.ItemId, r.Score);
                }
            }

            List<Rating> recommendations = new List<Rating>();
            foreach (int uId in table.Keys)
            {
                int counter = 0;
                foreach (Node n in rankedItems)
                {
                    if (!table.ContainsKey(uId, n.Id))  // u not rate
                    {
                        recommendations.Add(new Rating(uId, n.Id, 1.0));
                        counter++;
                    }
                    if (counter > k)
                    {
                        break;
                    }
                }
            }

            // Evaluation
            var pr = Metrics.PrecisionAndRecall(recommendations, testRatings);
            Console.WriteLine("{0}, {1}, {2}", k, pr.Item1, pr.Item2);
        }

        // Design for Facebook
        public void TryListRecommendation(List<Link> baseEdges, List<Link> testEdges, int userNumber, int k)
        {
            // Get u not rate item ralate value from Pk
            MyTable table = new MyTable();
            foreach (Link e in baseEdges)
            {
                if (!table.ContainsKey(e.From, e.To))
                {
                    table.Add(e.From, e.To, null);
                }
            }

            List<Link> recommendations = new List<Link>();
            foreach (int uId in table.Keys)
            {
                int counter = 0;
                foreach (Node n in this.Nodes)
                {
                    if (!table.ContainsKey(uId, n.Id))  // u not rate
                    {
                        recommendations.Add(new Link(uId, n.Id, 1.0));
                        counter++;
                    }
                    if (counter > k)
                    {
                        break;
                    }
                }
            }

            Console.Write("{0}, ", k);
            Evaluation(recommendations, testEdges);
        }


        public static void Evaluation(List<Link> recommendations, List<Link> test)
        {
            MyTable table = new MyTable();
            foreach (Link e in test)
            {
                if (!table.ContainsKey(e.From, e.To))
                {
                    table.Add(e.From, e.To, null);
                }
            }

            int hit = 0;
            foreach (Link e in recommendations)
            { 
                if (table.ContainsKey(e.From, e.To))
                {
                    hit++;
                }
            }

            Console.WriteLine("{0}, {1}", hit * 1.0 / recommendations.Count, hit * 1.0 / test.Count);

        }

    }   
}
