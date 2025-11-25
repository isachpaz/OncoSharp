// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using NLoptNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OncoSharp.SimplexGlobalSolver
{
    public class SimplexGlobalSolver
    {
        private readonly Func<double[], double> _objectiveFunction;
        private readonly List<(double Min, double Max)> _bounds;
        private readonly double _convergenceTolerance;
        private readonly int _maximumIterations;
        private readonly ILogger _logger;
        private readonly ConcurrentBag<SimplexResult> _solutions = new ConcurrentBag<SimplexResult>();

        private readonly Delegate _pinnedDelegate;

        public SimplexGlobalSolver(
            Func<double[], double> objectiveFunction,
            List<(double Min, double Max)> bounds,
            double convergenceTolerance = 1e-06,
            int maximumIterations = 10000,
            ILogger logger = null)
        {
            _objectiveFunction = objectiveFunction;
            _pinnedDelegate = _objectiveFunction;
            _bounds = bounds;
            _convergenceTolerance = convergenceTolerance;
            _maximumIterations = maximumIterations;
            _logger = logger;

            foreach (var bound in _bounds)
            {
                if (bound.Min > bound.Max)
                {
                    throw new ArgumentException("Each bound's Min value must be less than or equal to its Max value.");
                }
            }

            _logger?.LogInformation("SimplexGlobalSolver initialized successfully.");
            
        }

        public SimplexResult MaximizeWithMultiStart(int numInitialGuesses)
        {
            var initialGuesses = HammersleySequence.GeneratePoints(numInitialGuesses, _bounds);
            _logger.LogDebug(
                $"SimplexGlobalSolver - initialGuesses: {initialGuesses.Count}, " +
                $"Items: {string.Join(" | ", initialGuesses.Select(arr => $"[{string.Join(", ", arr)}]"))}"
            );

            foreach (var guess in initialGuesses)
            {
                _logger.LogDebug($"SimplexGlobalSolver - MaximizeFromInitialGuess with [{string.Join(", ", guess)}]");
                MaximizeFromInitialGuess(guess);
            }

            return GetBestResult();
        }

        public SimplexResult MaximizeWithParallelMultiStart(int numInitialGuesses)
        {
            var initialGuesses = HammersleySequence.GeneratePoints(numInitialGuesses, _bounds);

            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.ForEach(initialGuesses, options, guess =>
            {
                MaximizeFromInitialGuess(guess);
            });

            return GetBestResult();
        }

        private readonly ConcurrentDictionary<NLoptSolver, Delegate> _delegatePins = new ConcurrentDictionary<NLoptSolver, Delegate>();

        public SimplexResult MaximizeFromInitialGuess(double[] initialGuess)
        {
            var solver = new NLoptSolver(NLoptAlgorithm.LN_NELDERMEAD, (uint)_bounds.Count, _convergenceTolerance, _maximumIterations);

            solver.SetLowerBounds(BoundUtils.GetLowerBounds(_bounds));
            solver.SetUpperBounds(BoundUtils.GetUpperBounds(_bounds));

            // Pin the delegate
            var func = _objectiveFunction;
            solver.SetMaxObjective(func);
            _delegatePins[solver] = func; // ❗ Strong reference

            var result = solver.Optimize(initialGuess, out double? minf);
            _logger.LogDebug(
                $"Simplex Solve → InitialGuess: [{string.Join(", ", initialGuess)}], " +
                $"Result: [{string.Join(", ", result)}], MinF: {(minf?.ToString() ?? "null")}");

            if (minf != null)
            {
                _solutions.Add(new SimplexResult(initialGuess, minf.Value, result));
            }

            _delegatePins.TryRemove(solver, out _); // 🔓 Clean up

            var simplexSolution = new SimplexResult(initialGuess, minf.Value, result);
            return simplexSolution;

            //var algorithmName = NLoptAlgorithm.LN_NELDERMEAD;
            //var numOfVariables = (uint)_bounds.Count;
            //var solver = new NLoptSolver(algorithmName, numOfVariables, 1e-6, _maximumIterations);

            //solver.SetLowerBounds(BoundUtils.GetLowerBounds(_bounds));
            //solver.SetUpperBounds(BoundUtils.GetUpperBounds(_bounds));

            //solver.SetMaxObjective(_objectiveFunction);

            //Debug.WriteLine($"Initial guess: {string.Join(", ", initialGuess)}");
            //// Optimize
            //var result1 = solver.Optimize(initialGuess, out double? minf1);

            //if (minf1 != null)
            //{
            //    var simplexSolution = new SimplexResult(initialGuess, minf1.Value, result1);
            //    _solutions.Add(simplexSolution);
            //}
        }

        private SimplexResult GetBestResult()
        {
            if (_solutions.IsEmpty)
            {
                _logger?.LogWarning("No solutions have been found.");
                throw new InvalidOperationException("No solutions have been found.");
            }

            var bestSolution = _solutions.Aggregate((best, current) =>
                current.ObjectiveValue > best.ObjectiveValue ? current : best);

            _logger?.LogInformation($"Best solution found with value {bestSolution}.");

            return bestSolution;
        }
    }
}