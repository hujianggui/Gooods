using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;
using RS.Evaluation;
using RS.Data.Utility;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// The PageRank algorithm, used for top-N recommendation.
    /// http://blog.jobbole.com/71431/
    /// </summary>
    public class PageRank
    {
        public double[] P0 { get; protected set; } // 0-based, initial value, V0

        public double[] PR { get; protected set; } // 0-based, final result

        public PageRank(int nodes)
        {
            P0 = new double[nodes];
            PR = new double[nodes];
        }

        /// <summary>
        /// $$V_{t+1} = \alpha * M * V_t + (1 - \alpha) * V_t$$
        /// </summary>
        /// <param name="links">matrix M, note that M[i][j] denotes a link from j -> i.</param>
        /// <param name="rank">column vector V_t</param>
        /// <param name="alpha">restart probability</param>
        /// <returns></returns>
        protected double[] Iterate(List<Link> links, double[] rank, double alpha = 1.0)
        {
            double[] result = new double[rank.Length];
            foreach (Link l in links)
            {
                result[l.To] += (l.Weight * rank[l.From]);
            }

            if (alpha > 0)
            {
                for (int i = 0; i < rank.Length; i++)
                {
                    if (P0[i] > 0)
                    {
                        result[i] = alpha * result[i] + (1.0 - alpha) * P0[i];
                    }
                }
            }
            return result;
        }

        public void Train(List<Link> train, int epochs = 20, double alpha = 0.8, double convergency = 1e-6)
        {
            var outLinksTable = Tools.GetUserLinksTable(train);

            // Update weights for edges, and a weight of an edge is the inverse of #(out links). 
            // Matrix M
            foreach (Link e in train)
            {
                List<Link> outlinks = (List<Link>)outLinksTable[e.From];
                e.Weight = 1.0 / outlinks.Count;
            }

            // Initialize V0
            foreach (int f in outLinksTable.Keys)
            {
                P0[f] = PR[f] = 1.0 / outLinksTable.Count;
            }

            Console.WriteLine("Epoch,S");
            
            for(int epoch = 1; epoch <= epochs; epoch++)
            {
                double[] PR1 = Iterate(train, PR, alpha);
                double ssd = MathUtility.SumOfSquaredDifference(PR, PR1);
                Console.WriteLine("{0},{1:f10}", epoch, ssd);
                // Console.WriteLine("{0},{1:f6},{2:f6},{3:f6},{4:f6},{5:f6}", epoch, ssd, PR1[0], PR1[1], PR1[2], PR1[3]);
                PR = PR1;

                if (ssd < convergency)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Get item nodes and sort them by descending
        /// </summary>
        /// <param name="rank">ranking values for all items.</param>
        /// <param name="maxUserId">item id = index of rank[] - (maxUserId + 1)</param>
        /// <returns></returns>
        protected List<Node> GetItemNodes(double[] rank, int maxUserId)
        {
            List<Node> nodes = new List<Node>();
            for (int i = maxUserId + 1; i < PR.Length; i++)
            {
                nodes.Add(new Node(i - maxUserId - 1, PR[i]));
            }
            return nodes.OrderByDescending(n => n.Weight).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ratingTable">table: user id - item id - score</param>
        /// <param name="items">list of sorted items</param>
        /// <param name="N">length of recommendations</param>
        /// <returns></returns>
        protected List<Rating> GetRecommendations(MyTable ratingTable, List<Node> items, int N = 30)
        {
            List<Rating> list = new List<Rating>();
            foreach (int userId in ratingTable.Keys)
            {
                int counter = 1;
                foreach (Node n in items)
                {
                    if (!ratingTable.ContainsKey(userId, n.Id))  // u not rate
                    {
                        list.Add(new Rating(userId, n.Id, 1.0));
                        counter++;
                    }
                    if (counter > N)
                    {
                        break;
                    }
                }
            }
            return list;
        }

        public void PrintParameters(List<Link> trainLinks, List<Rating> train, List<Rating> test,
            int epochs = 20, double alpha = 0.8, double convergency = 1e-6)
        {
            Console.WriteLine(GetType().Name);
            Console.WriteLine("trainLinks,{0}", trainLinks.Count);
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("alpha,{0}", alpha);
            Console.WriteLine("convergency,{0}", convergency);
        }

        public void TryTopN(List<Link> trainLinks, List<Rating> train, List<Rating> test, int maxUserId, 
            int epochs = 20, double alpha = 0.8, double convergency = 1e-6)
        {
            PrintParameters(trainLinks, train, test, epochs, alpha, convergency);

            var outLinksTable = Tools.GetUserLinksTable(trainLinks);

            // Update weights for edges, and a weight of an edge is the inverse of #(out links). 
            // Matrix M
            foreach (Link e in trainLinks)
            {
                List<Link> outlinks = (List<Link>)outLinksTable[e.From];
                e.Weight = 1.0 / outlinks.Count;
            }

            // Initialize V0
            foreach (int f in outLinksTable.Keys)
            {
                P0[f] = PR[f] = 1.0 / outLinksTable.Count;
            }

            Console.WriteLine("Epoch#S,N,P,R,Coverage,Popularity,MAP");
            MyTable userItemTable = Tools.GetRatingTable(train);
            int[] Ns = { 1, 5, 10, 15, 20, 25, 30 }; 

            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                double[] PR1 = Iterate(trainLinks, PR, alpha);
                double ssd = MathUtility.SumOfSquaredDifference(PR, PR1);
                //Console.Write("{0}#{1:f10}", epoch, ssd);
                Console.Write("{0}", epoch);
                PR = PR1;

                // Evaluate
                var itemRanks = GetItemNodes(PR, maxUserId);
                var recommendations = GetRecommendations(userItemTable, itemRanks, Ns[Ns.Length - 1]);
                foreach (int n in Ns)
                {
                    Console.Write(",{0}", n);
                    List<Rating> subset = Tools.GetSubset(recommendations, n);
                    var pr = Metrics.PrecisionAndRecall(subset, test);
                    var cp = Metrics.CoverageAndPopularity(subset, train);
                    var map = Metrics.MAP(subset, test, n);
                    Console.WriteLine(",{0},{1},{2},{3},{4}", pr.Item1, pr.Item2, cp.Item1, cp.Item2, map);
                }

                if (ssd < convergency)
                {
                    break;
                }
            }
        }

        public static void Example()
        {
            List<Link> links = new List<Link>();
            links.Add(new Link(0, 1));
            links.Add(new Link(0, 2));
            links.Add(new Link(0, 3));
            links.Add(new Link(1, 0));
            links.Add(new Link(1, 3));
            links.Add(new Link(2, 0));  // try to comment this edge? all PR would be zero. A missing problem
            links.Add(new Link(3, 1));
            links.Add(new Link(3, 2));

            PageRank pr = new PageRank(4);
            pr.Train(links, 100);
        }

        public static void Example2()
        {
            List<Link> links = new List<Link>();
            links.Add(new Link(0, 1));
            links.Add(new Link(0, 2));
            links.Add(new Link(0, 3));
            links.Add(new Link(1, 0));
            links.Add(new Link(1, 3));
            links.Add(new Link(2, 2));  // note, this needs to be restart. A trapping problem.
            links.Add(new Link(3, 1));
            links.Add(new Link(3, 2));

            PageRank pr = new PageRank(4);
            pr.Train(links, 100);
        }
    }
}
