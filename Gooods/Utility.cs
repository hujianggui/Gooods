using System;

namespace Gooods
{
    public static class Utility
    {
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
