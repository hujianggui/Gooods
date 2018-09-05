
using System;

namespace Gooods.DataType
{
    public class Random : System.Random
    {
        [ThreadStatic]
        private static Random instance;

        private static Nullable<int> seed;

        /// <summary>Default constructor</summary>
        private Random() : base() { }

        /// <summary>Creates a Random object initialized with a seed</summary>
        /// <param name="seed">An integer for initializing the random number generator</param>
        private Random(int seed) : base(seed) { }

        /// <summary>the random seed</summary>
        public static int Seed
        {
            set
            {
                Console.Error.WriteLine("Set random seed to {0}.", value);
                seed = value;
                instance = new Random(seed.Value);
            }
        }

        /// <summary>Gets the instance. If it does not exist yet, it will be created.</summary>
        /// <returns>the singleton instance</returns>
        public static Random GetInstance()
        {
            if (instance == null)
                Init();
            return instance;
        }

        /// <summary>(Re-)initialize the instance</summary>
        public static void Init()
        {
            if (seed == null)
                instance = new Random();
            else
                instance = new Random(seed.Value);
        }

        public double Gaussian()
        {
            // Joseph L. Leva: A fast normal Random number generator
            double u, v, x, y, Q;
            Random random = GetInstance();
            do
            {
                do
                {
                    u = random.NextDouble();
                } while (u == 0.0);

                v = 1.7156 * (random.NextDouble() - 0.5);
                x = u - 0.449871;
                y = Math.Abs(v) + 0.386595;
                Q = x * x + y * (0.19600 * y - 0.25472 * x);
                if (Q < 0.27597)
                {
                    break;
                }
            } while ((Q > 0.27846) || ((v * v) > (-4.0 * u * u * System.Math.Log(u))));
            return v / u;
        }

        public double Gaussian(double mean, double stdev)
        {
            if ((stdev == 0.0) || (double.IsNaN(stdev)))
            {
                return mean;
            }
            else
            {
                return mean + stdev * Gaussian();
            }
        }

    }
}
