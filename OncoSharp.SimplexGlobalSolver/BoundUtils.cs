// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using System.Linq;

namespace OncoSharp.SimplexGlobalSolver
{
    public static class BoundUtils
    {
        public static double[] GetLowerBounds(List<(double Min, double Max)> bounds)
        {
            return bounds.Select(b => b.Min).ToArray();
        }

        public static double[] GetUpperBounds(List<(double Min, double Max)> bounds)
        {
            return bounds.Select(b => b.Max).ToArray();
        }
        public static double[] GetMidBounds(List<(double Min, double Max)> bounds)
        {
            return bounds.Select(b => (b.Min + b.Max) / 2.0).ToArray();
        }
    }
}