// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Optimization.Abstractions.Models;


namespace OncoSharp.Optimization.Abstractions.Interfaces
{
    public interface IOptimizer
    {
        IOptimizer SetMaxObjective(Func<double[], double> objective);
        IOptimizer SetLowerBounds(double[] lowerBounds);
        IOptimizer SetUpperBounds(double[] upperBounds);
        OptimizationResult Maximize(double[] initialGuess);
    }
}