using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RS.Data.Utility;
using RS.DataType;
using RS.Evaluation;
using System.Collections;

namespace RS.CollaborativeFiltering
{
    /// <summary>
    /// ICDM2011, p497, Ning
    /// SLIM: Sparse Linear Methods for Top-N Recommender Systems
    /// 1. 全量 2. kNN
    /// </summary>
    public class SLIM
    {
        public double[,] W { get; private set; }   // n * n

        protected Hashtable UserItemsTable { get; private set; }

        protected Hashtable ItemUsersTable { get; private set; }

        protected Hashtable SimilarItemsTable { get; private set; }

        protected MyTable RatingTable { get; private set; }

        /// <summary>
        /// K: if K = 0, do not use itemKNN; if K > 0, # of item neighbors
        /// </summary>
        protected int K { get; private set; }

        /// <summary>Regularization parameter for the L1 regularization term (lambda in the original paper)</summary>
        public double RegL1 { get { return reg_l1; } set { reg_l1 = value; } }
        /// <summary>Regularization parameter for the L1 regularization term (lambda in the original paper)</summary>
        protected double reg_l1 = 0.01;

        /// <summary>Regularization parameter for the L2 regularization term (beta/2 in the original paper)</summary>
        public double RegL2 { get { return reg_l2; } set { reg_l2 = value; } }
        /// <summary>Regularization parameter for the L2 regularization term (beta/2 in the original paper)</summary>
        protected double reg_l2 = 0.001;


        public SLIM() { }
        public SLIM(int m, int n)
        {
            InitializeModel(m, n);
        }

        public void InitializeModel(int m, int n)
        {
            W = MathUtility.RandomGaussian(n, n, 0, 0.1);
            for (int i = 0; i < n; i++)
            {
                W[i, i] = 0.0;
            }
        }

        public double Predict(int userId, int itemId, int excludeItemId)
        {
            double sum = 0.0; // A ranking score

            if (K > 0)
            {
                List<Link> items = (List<Link>)SimilarItemsTable[itemId];
                foreach (Link r in items)
                {
                    // this user had rated this similar item.
                    if (r.To != excludeItemId && RatingTable.ContainsKey(userId, r.To))
                    {
                        sum += W[itemId, r.To];
                    }
                }
            }
            else
            {
                List<Rating> items = (List<Rating>)UserItemsTable[userId];
                foreach(Rating r in items)
                {
                    if (r.ItemId != excludeItemId)
                    {
                        sum += W[itemId, r.ItemId];
                    }                    
                }
            }

            return sum;
        }

        protected void UpdateParameters(int itemId, int otherItemId)
        {
            List<Rating> users = (List<Rating>)ItemUsersTable[otherItemId];
            double gradientSum = 0;
            foreach(Rating r in users)
            {
                // a user had rate itemId and otherItemId
                if (RatingTable.ContainsKey(r.UserId, itemId))
                {
                    gradientSum += 1;
                }
                gradientSum -= Predict(r.UserId, itemId, otherItemId);  // other item id is a similar item
            }
            double gradient = gradientSum / (UserItemsTable.Count + 1.0);
            if (reg_l1 < Math.Abs(gradient))
            {
                if (gradient > 0)
                {
                    double update = (gradient - reg_l1) / (1.0 + reg_l2);
                    W[itemId, otherItemId] = (float)update;
                }
                else
                {
                    double update = (gradient + reg_l1) / (1.0 + reg_l2);
                    W[itemId, otherItemId] = (float)update;
                }
            }
            else
            {
                W[itemId, otherItemId] = 0;
            }
        }

        /// <summary>Perform one iteration of coordinate descent for a given set of item parameters over the training data</summary>
        public void Iterate(int itemId)
        {
            if (K > 0)
            {
                List<Link> items = (List<Link>)SimilarItemsTable[itemId];   
                foreach (Link r in items)
                {
                    if (r.To != itemId)
                    {
                        UpdateParameters(itemId, r.To);
                    }                        
                }
            }
            else
            {
                foreach(int feat in ItemUsersTable.Keys)
                {
                    if (feat != itemId)
                    {
                        UpdateParameters(itemId, feat);
                    }
                }                   
            }
        }

        protected List<Rating> GetRecommendations(MyTable ratingTable, int N = 30)
        {
            List<Rating> recommendedItems = new List<Rating>();
            ArrayList list = ratingTable.GetSubKeyList();
           
            int[] mainKeys = new int[ratingTable.Keys.Count];
            ratingTable.Keys.CopyTo(mainKeys, 0);

            Parallel.ForEach(mainKeys, userId =>
            {
                Hashtable Nu = (Hashtable)ratingTable[userId];      // ratings of user u
                List<Rating> predictedRatings = new List<Rating>();
                foreach (int itemId in list)
                {
                    if (!Nu.ContainsKey(itemId))
                    {
                        double p = Predict(userId, itemId, -1);
                        predictedRatings.Add(new Rating(userId, itemId, p));
                    }
                }
                List<Rating> sortedLi = predictedRatings.OrderByDescending(r => r.Score).ToList();
                var selectedLi = sortedLi.GetRange(0, Math.Min(sortedLi.Count, N));
                lock (recommendedItems)
                {
                    recommendedItems.AddRange(selectedLi);
                }
            });

            return recommendedItems;
        }

        public void TryLeastSquare(List<Rating> train, List<Rating> test, int epochs = 10, double reg_l1 = 0.01, double reg_l2 = 0.001)
        {
            Console.WriteLine("train,{0}", train.Count);
            Console.WriteLine("test,{0}", test.Count);
            Console.WriteLine("epochs,{0}", epochs);
            Console.WriteLine("reg_l1,{0}", reg_l1);
            Console.WriteLine("reg_l2,{0}", reg_l2);

            this.reg_l1 = reg_l1;
            this.reg_l2 = reg_l2;

            UserItemsTable = Tools.GetUserItemsTable(train);
            ItemUsersTable = Tools.GetItemUsersTable(train);
            RatingTable    = Tools.GetRatingTable(train);

            K = 80;

            ItemKNNv2 itemkNN = new ItemKNNv2();
            MyTable coourrenceTable = itemkNN.CalculateCooccurrences(UserItemsTable, true);
            MyTable wuv = itemkNN.CalculateSimilarities(coourrenceTable, ItemUsersTable);
            SimilarItemsTable = itemkNN.GetSimilarItems(wuv, K);

            int[] Ns = {1, 5, 10, 15, 20, 25, 30};

            int[] itemIds = new int[ItemUsersTable.Count];
            ItemUsersTable.Keys.CopyTo(itemIds, 0);
            
            for(int e = 1; e <= epochs; e++)
            {

                Hashtable counter = new Hashtable();
                counter.Add(0, 0);

                Parallel.ForEach(itemIds, itemId =>
                {
                    Iterate(itemId);

                    lock (counter)
                    {
                        counter[0] = (int)counter[0] + 1;
                        Console.Write("{0},{1:f6}", e, (int)counter[0] * 1.0 / ItemUsersTable.Count);
                        Console.SetCursorPosition(0, Console.CursorTop);
                    }
                });

                if (e % 2 == 0 && e >= 10)
                {
                    Console.Write("{0}", e);
                    List<Rating> recommendedRatings = GetRecommendations(RatingTable, 30);
                    foreach(int n in Ns)
                    {
                        List<Rating> subset = Tools.GetSubset(recommendedRatings, n);
                        var pr = Metrics.PrecisionAndRecall(subset, test);
                        var cp = Metrics.CoverageAndPopularity(subset, train);
                        var map = Metrics.MAP(subset, test, n);
                        Console.WriteLine(",{0},{1},{2},{3},{4},{5}", n, pr.Item1, pr.Item2, cp.Item1, cp.Item2, map);
                    }  
                }               
            }
        }

    }
}
