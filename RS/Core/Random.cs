using System;

namespace RS.Core
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
    }
}
