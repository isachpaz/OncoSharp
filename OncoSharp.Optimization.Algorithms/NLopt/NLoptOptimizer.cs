// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using NLoptNet;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Abstractions.Models;
using OncoSharp.Optimization.Algorithms.NLopt.OncoSharp.Optimization.Algorithms.NLopt;
using System;

namespace OncoSharp.Optimization.Algorithms.NLopt
{
    public class NLoptOptimizer : IOptimizer
    {
        private readonly NLoptSolver _solver;

        public NLoptOptimizer(NLoptAlgorithm algorithm, int parameterCount, double xtolRel, int maxEval)
        {
            _solver = new NLoptSolver(algorithm, (uint)parameterCount, xtolRel, maxEval);
        }

        public IOptimizer SetMaxObjective(Func<double[], double> objective)
        {
            _solver.SetMaxObjective((x, grad) => objective(x));
            return this;
        }

        public IOptimizer SetLowerBounds(double[] lowerBounds)
        {
            _solver.SetLowerBounds(lowerBounds);
            return this;
        }

        public IOptimizer SetUpperBounds(double[] upperBounds)
        {
            _solver.SetUpperBounds(upperBounds);
            return this;
        }

        public OptimizationResult Maximize(double[] initialGuess)
        {
            var result = _solver.Optimize(initialGuess, out var optVal);
            return new OptimizationResult(initialGuess, optVal ?? double.NaN, NloptResultMapper.MapToExitStatus(result));
        }

        public OptimizationResult MaximizeFromSingleStart(double[] initialGuess)
        {
            return Maximize(initialGuess);
        }
    }
}