// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Linq;

namespace OncoSharp.Core.Quantities.Helpers.Maths
{
    public static class MathUtils
    {
        public static int CountDecimalDigits(double n)
        {
            return n.ToString(System.Globalization.CultureInfo.InvariantCulture)
                //.TrimEnd('0') uncomment if you don't want to count trailing zeroes
                .SkipWhile(c => c != '.')
                .Skip(1)
                .Count();
        }

        /// <summary>
        /// Code: https://www.johndcook.com/blog/csharp_erf/
        /// The implementation is based on "Handbook of Mathematical Functions:
        /// with Formulas, Graphs, and Mathematical Tables (Dover Books on Mathematics)"
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
    }
}