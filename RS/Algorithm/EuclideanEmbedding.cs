using System;
using System.Collections.Generic;
using System.Linq;

using RS.DataType;

namespace RS.Algorithm
{
    /// <summary>
    /// class EuclideanEmbedding
    /// Collaborative Filtering via Euclidean Embedding
    /// Recsys 2010, p87, Khoshneshin & Street
    /// </summary>
    public class EuclideanEmbedding : BiasedMatrixFactorization
    {
        public EuclideanEmbedding() { }

        public EuclideanEmbedding(int p, int q, int f = 10)
        {
            InitializeModel(p, q, f);
        }

        public override double Predict(int userId, int itemId, double miu)
        {
            double _r = 0.0;
            for (int i = 0; i < this.f; i++)
            {
                double e = P[userId, i] - Q[itemId, i];
                _r += e * e;
            }
            return bu[userId] + bi[itemId] + miu - _r;
        }

        public override void TrySGD(List<Rating> train, List<Rating> test, int epochs = 100, double gamma = 0.01, double lambda = 0.01, double decay = 1.0, double mimimumRating = 1.0, double maximumRating = 5.0)
        {
            PrintParameters(train, test, epochs, gamma, lambda, decay, mimimumRating, maximumRating);
            double miu = train.AsParallel().Average(r => r.Score);
            Console.WriteLine("epoch,loss,test:mae,test:rmse");

            double loss = Loss(test, lambda, miu);

            for (int iter = 0; iter < epochs; iter++)
            {
                foreach (Rating r in train)
                {
                    double pui = Predict(r.UserId, r.ItemId, miu);
                    double eui = r.Score - pui;
                    bu[r.UserId] += gamma * (eui - lambda * bu[r.UserId]);
                    bi[r.ItemId] += gamma * (eui - lambda * bi[r.ItemId]);

                    for (int i = 0; i < f; i++)
                    {
                        P[r.UserId, i] -= (gamma * (P[r.UserId, i] - Q[r.ItemId, i]) * (eui + lambda)); 
                        Q[r.ItemId, i] += (gamma * (P[r.UserId, i] - Q[r.ItemId, i]) * (eui + lambda));                        
                    }
                }

                double lastLoss = Loss(test, lambda, miu);
                var eval = EvaluateMaeRmse(test, miu);
                Console.WriteLine("{0},{1},{2},{3}", iter + 1, lastLoss, eval.Item1, eval.Item2);

                if (decay != 1.0)
                {
                    gamma *= decay;
                }                
                if (lastLoss < loss)
                {
                    loss = lastLoss;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
