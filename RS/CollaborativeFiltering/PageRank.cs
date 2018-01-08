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
                result[l.To] += l.Weight * rank[l.From];
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

        public void Train(List<Link> train, int epochs = 20, double alpha = 0.8, double convergency = 1e-5)
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
                double differences = MathUtility.SumOfSquaredDifference(PR, PR1);
                Console.WriteLine("{0},{1:f10}", epoch, differences);
                //Console.WriteLine("{0},{1:f6},{2:f6},{3:f6},{4:f6},{5:f6}", epoch, differences, PR1[0], PR1[1], PR1[2], PR1[3]);
                PR = PR1;
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
            links.Add(new Link(2, 0));  // try to comment this edge? all PR would be zero
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
            links.Add(new Link(2, 2));  // note, this needs to be restart. RWR
            links.Add(new Link(3, 1));
            links.Add(new Link(3, 2));

            PageRank pr = new PageRank(4);
            pr.Train(links, 100);
        }
    }
}
