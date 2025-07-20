// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Abstractions.Models;
using OncoSharp.Optimization.Algorithms.NLopt.OncoSharp.Optimization.Algorithms.NLopt;

namespace OncoSharp.Optimization.Algorithms.Simplex
{
    public class SimplexGlobalOptimizer : IOptimizer
    {
        public int NumberOfMultipleStarts { get; }
        private Func<double[], double> _objectiveFunction = null;
        private Func<double[], double> _pinnedObjectiveFunction;
        private double[] _lowerBounds = null;
        private double[] _upperBounds = null;
        private readonly double _convergenceTolerance = 1e-6;
        private readonly int _maximumIterations = 10000;
        private readonly ILogger _logger;

        public SimplexGlobalOptimizer(int numberOfMultipleStarts = 50, ILogger logger = null)
        {
            NumberOfMultipleStarts = numberOfMultipleStarts;
            _logger = logger;
        }

        public IOptimizer SetMaxObjective(Func<double[], double> objective)
        {
            _objectiveFunction = objective ?? throw new ArgumentNullException(nameof(objective));
            _pinnedObjectiveFunction = _objectiveFunction; // keeps a strong reference to avoid GC

            return this;
        }

        public IOptimizer SetLowerBounds(double[] lowerBounds)
        {
            _lowerBounds = lowerBounds ?? throw new ArgumentNullException(nameof(lowerBounds));
            return this;
        }

        public IOptimizer SetUpperBounds(double[] upperBounds)
        {
            _upperBounds = upperBounds ?? throw new ArgumentNullException(nameof(upperBounds));
            return this;
        }

        public OptimizationResult Maximize(double[] initialGuess)
        {
            if (_objectiveFunction == null) throw new InvalidOperationException("Objective function must be set.");
            if (_lowerBounds == null || _upperBounds == null)
                throw new InvalidOperationException("Bounds must be set.");
            if (initialGuess == null) throw new ArgumentNullException(nameof(initialGuess));
            if (initialGuess.Length != _lowerBounds.Length || initialGuess.Length != _upperBounds.Length)
                throw new ArgumentException("Initial guess must match the number of dimensions.");

            // Convert bounds to tuple format
            var bounds = new List<(double Min, double Max)>();
            for (int i = 0; i < _lowerBounds.Length; i++)
            {
                bounds.Add((_lowerBounds[i], _upperBounds[i]));
            }

            // Instantiate AdaptiveSimplex
            var adaptiveSimplex = new SimplexGlobalSolver.SimplexGlobalSolver(
                _pinnedObjectiveFunction,
                bounds,
                _convergenceTolerance,
                _maximumIterations,
                _logger);


            //var solution = adaptiveSimplex.MaximizeFromInitialGuess(initialGuess);
            var solution = adaptiveSimplex.MaximizeWithMultiStart(NumberOfMultipleStarts);

            return new OptimizationResult(solution.Points.ToArray(), solution.ObjectiveValue,
                NloptResultMapper.MapToExitStatus(solution.ExitReason));


        }

        public OptimizationResult MaximizeFromSingleStart(double[] initialGuess)
        {
            if (_objectiveFunction == null) throw new InvalidOperationException("Objective function must be set.");
            if (_lowerBounds == null || _upperBounds == null)
                throw new InvalidOperationException("Bounds must be set.");
            if (initialGuess == null) throw new ArgumentNullException(nameof(initialGuess));
            if (initialGuess.Length != _lowerBounds.Length || initialGuess.Length != _upperBounds.Length)
                throw new ArgumentException("Initial guess must match the number of dimensions.");

            // Convert bounds to tuple format
            var bounds = new List<(double Min, double Max)>();
            for (int i = 0; i < _lowerBounds.Length; i++)
            {
                bounds.Add((_lowerBounds[i], _upperBounds[i]));
            }

            // Instantiate AdaptiveSimplex
            var adaptiveSimplex = new SimplexGlobalSolver.SimplexGlobalSolver(
                _pinnedObjectiveFunction,
                bounds,
                _convergenceTolerance,
                _maximumIterations,
                _logger);


            var solution = adaptiveSimplex.MaximizeFromInitialGuess(initialGuess);


            return new OptimizationResult(solution.Points.ToArray(), solution.ObjectiveValue,
                NloptResultMapper.MapToExitStatus(solution.ExitReason));

        }
    }
}