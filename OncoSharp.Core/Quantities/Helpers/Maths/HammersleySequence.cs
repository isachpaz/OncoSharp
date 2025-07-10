// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.Helpers.Maths
{
    using System;
    using System.Collections.Generic;

  public class HammersleySequence
    {
        /// <summary>
        /// Generate the Hammersley sequence for a given number of points, dimensions, and bounds.
        /// Example:
        ///     var bounds = new List<(double, double)> { (0, 1), (0, 5), (10, 20) };
        ///     var points = HammersleySequence.GeneratePoints(100, bounds);
        ///     var points3D = HammersleySequence.Generate3DPoints(50);
        /// </summary>
        /// <param name="numPoints">Number of points to generate</param>
        /// <param name="bounds">List of (Min, Max) tuples for each dimension</param>
        /// <returns>List of points as double arrays</returns>
        public static List<double[]> GeneratePoints(int numPoints, List<Tuple<double, double>> bounds)
        {
            if (bounds == null)
                throw new ArgumentNullException("bounds");
            if (numPoints <= 0)
                throw new ArgumentOutOfRangeException("numPoints", "Number of points must be positive.");

            int dimensions = bounds.Count;
            if (dimensions == 0)
                throw new ArgumentException("Bounds list cannot be empty.", "bounds");

            var points = new List<double[]>(numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                double[] point = new double[dimensions];
                point[0] = (double)i / numPoints;

                for (int j = 1; j < dimensions; j++)
                {
                    point[j] = RadicalInverse(i, GetPrime(j));
                }

                for (int j = 0; j < dimensions; j++)
                {
                    double min = bounds[j].Item1;
                    double max = bounds[j].Item2;
                    point[j] = min + point[j] * (max - min);
                }

                points.Add(point);
            }

            return points;
        }

        /// <summary>
        /// Generate the Hammersley sequence for a given number of points in 3 dimensions.
        /// </summary>
        /// <param name="numPoints">Number of points to generate</param>
        /// <returns>List of points as double arrays</returns>
        public static List<double[]> Generate3DPoints(int numPoints)
        {
            if (numPoints <= 0)
                throw new ArgumentOutOfRangeException("numPoints", "Number of points must be positive.");

            const int dimensions = 3;
            var points = new List<double[]>(numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                double[] point = new double[dimensions];
                point[0] = (double)i / numPoints;

                for (int j = 1; j < dimensions; j++)
                {
                    point[j] = RadicalInverse(i, GetPrime(j));
                }

                points.Add(point);
            }

            return points;
        }

        /// <summary>
        /// Compute the radical inverse of an integer n in the specified base.
        /// </summary>
        private static double RadicalInverse(int n, int baseValue)
        {
            double inverse = 0.0;
            double baseInv = 1.0 / baseValue;
            double basePow = baseInv;

            while (n > 0)
            {
                int digit = n % baseValue;
                inverse += digit * basePow;
                n /= baseValue;
                basePow *= baseInv;
            }

            return inverse;
        }

        /// <summary>
        /// Get the nth prime number (0-indexed).
        /// Supports generating larger primes beyond the initial list.
        /// </summary>
        private static int GetPrime(int index)
        {
            int[] primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71 };

            if (index < primes.Length)
                return primes[index];

            var primeList = new List<int>(primes);
            int candidate = primeList[primeList.Count - 1] + 2;

            while (primeList.Count <= index)
            {
                if (IsPrime(candidate, primeList))
                {
                    primeList.Add(candidate);
                }
                candidate += 2;
            }

            return primeList[index];
        }

        private static bool IsPrime(int number, List<int> knownPrimes)
        {
            int limit = (int)Math.Sqrt(number);
            foreach (int p in knownPrimes)
            {
                if (p > limit)
                    break;
                if (number % p == 0)
                    return false;
            }
            return true;
        }
    }
}