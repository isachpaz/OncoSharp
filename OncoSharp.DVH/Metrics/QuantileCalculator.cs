// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OncoSharp.DVH.Metrics
{
    public class QuantileCalculator
    {
        /// <summary>
        /// Computes the quantile using R's type=7 algorithm.
        /// </summary>
        /// <param name="data">The data vector</param>
        /// <param name="p">The desired quantile (e.g., 0.25, 0.5, 0.75)</param>
        /// <returns>The interpolated quantile value</returns>
        public double QuantileType7(IReadOnlyList<double> data, double p)
        {
            return QuantileType7(data, new double[] { p }).First();
        }

        public double[] QuantileType7(IReadOnlyList<double> data, double[] p)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Data must not be empty.", nameof(data));

            if (p == null || p.Length == 0)
                throw new ArgumentException("Quantile probabilities must not be empty.", nameof(p));

            foreach (var prob in p)
            {
                if (prob < 0 || prob > 1)
                    throw new ArgumentOutOfRangeException(nameof(p),
                        "Each quantile probability must be between 0 and 1.");
            }

            var sorted = data.OrderBy(x => x).ToList();
            int n = sorted.Count;


            if (n == 1)
                return Enumerable.Repeat(sorted[0], p.Length).ToArray();

            double[] results = new double[p.Length];

            for (int i = 0; i < p.Length; i++)
            {
                double prob = p[i];
                double h = 1 + (n - 1) * prob;
                int j = (int)Math.Floor(h) - 1;
                double gamma = h - Math.Floor(h);

                if (j < 0)
                    results[i] = sorted[0];
                else if (j >= n - 1)
                    results[i] = sorted[n - 1];
                else
                    results[i] = (1 - gamma) * sorted[j] + gamma * sorted[j + 1];
            }

            return results;
        }
    }
}