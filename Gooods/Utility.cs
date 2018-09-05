using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Gooods
{
    public static class Utility
    {
        /// <summary>Shuffle a list in-place</summary>
        /// <remarks>
        /// Fisher-Yates shuffle, see
        /// http://en.wikipedia.org/wiki/Fisher–Yates_shuffle
        /// </remarks>
        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = DataType.Random.GetInstance();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                int r = random.Next(i + 1);

                // swap position i with position r
                T tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
        }

        /// <summary>Get all types in a namespace</summary>
        /// <param name="name_space">a string describing the namespace</param>
        /// <returns>a list of Type objects</returns>
        public static IList<Type> GetTypes(string name_space)
        {
            var types = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                types.AddRange(assembly.GetTypes().Where(t => string.Equals(t.Namespace, name_space, StringComparison.Ordinal)));

            return types;
        }

        /// <summary>Measure how long an action takes</summary>
        /// <param name="t">An <see cref="Action"/> defining the action to be measured</param>
        /// <returns>The <see cref="TimeSpan"/> it takes to perform the action</returns>
        public static TimeSpan MeasureTime(Action t)
        {
            DateTime startTime = DateTime.Now;
            t(); // perform task
            return DateTime.Now - startTime;
        }


        /// <summary>
        /// Logistic function.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Logistic(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }
    }
}
