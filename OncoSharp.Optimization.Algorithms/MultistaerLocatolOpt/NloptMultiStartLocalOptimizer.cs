// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Optimization.Algorithms.MultistaerLocatolOpt
{

    // Optional logging
    using Microsoft.Extensions.Logging;
    // Adjust namespace to your NLoptNet package
    using NLoptNet;
    using OncoSharp.Optimization.Abstractions.Interfaces;
    using OncoSharp.Optimization.Abstractions.Models;
    using OncoSharp.Optimization.Algorithms.NLopt.OncoSharp.Optimization.Algorithms.NLopt;
    using System;
    using System.Linq;

    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using NLoptNet; // adjust if your namespace differs

    public class NloptMultiStartLocalOptimizer : IOptimizer
    {
        public int NumberOfMultipleStarts { get; private set; }

        private Func<double[], double> _objectiveFunction;
        private Func<double[], double> _pinnedObjectiveFunction;

        private double[] _lowerBounds;
        private double[] _upperBounds;

        private readonly double _relativeStoppingTolerance;
        private readonly int _maximumIterations;

        private readonly ILogger _logger;
        private readonly Random _rng;

        public NloptMultiStartLocalOptimizer(
            int numberOfMultipleStarts = 50,
            double relativeStoppingTolerance = 1e-6,
            int maximumIterations = 10_000,
            int? seed = null,
            ILogger logger = null)
        {
            if (numberOfMultipleStarts <= 0) numberOfMultipleStarts = 1;

            NumberOfMultipleStarts = numberOfMultipleStarts;
            _relativeStoppingTolerance = relativeStoppingTolerance;
            _maximumIterations = maximumIterations;
            _logger = logger;

            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public IOptimizer SetMaxObjective(Func<double[], double> objective)
        {
            if (objective == null) throw new ArgumentNullException(nameof(objective));
            _objectiveFunction = objective;
            _pinnedObjectiveFunction = _objectiveFunction; // keep strong reference
            return this;
        }

        public IOptimizer SetLowerBounds(double[] lowerBounds)
        {
            if (lowerBounds == null) throw new ArgumentNullException(nameof(lowerBounds));
            _lowerBounds = lowerBounds;
            return this;
        }

        public IOptimizer SetUpperBounds(double[] upperBounds)
        {
            if (upperBounds == null) throw new ArgumentNullException(nameof(upperBounds));
            _upperBounds = upperBounds;
            return this;
        }

        public OptimizationResult Maximize(double[] initialGuess)
        {
            ValidateInputs(initialGuess);

            // Include the provided initial guess as start #0
            var starts = new double[NumberOfMultipleStarts][];
            starts[0] = (double[])initialGuess.Clone();

            for (int i = 1; i < NumberOfMultipleStarts; i++)
                starts[i] = SampleUniformWithinBounds();

            return RunMultiStart(starts);
        }

        public OptimizationResult MaximizeFromSingleStart(double[] initialGuess)
        {
            ValidateInputs(initialGuess);

            var starts = new double[1][];
            starts[0] = (double[])initialGuess.Clone();

            return RunMultiStart(starts);
        }

        // ---------------- core ----------------

        private OptimizationResult RunMultiStart(double[][] starts)
        {
            double bestObj = double.NegativeInfinity;
            double[] bestX = null;
            NloptResult bestExit = NloptResult.FAILURE;

            for (int i = 0; i < starts.Length; i++)
            {
                var x0 = starts[i];

                // Try BOBYQA first
                double[] x1;
                double obj1;
                NloptResult exit1;
                bool ok1 = TryLocalMaximize(NLoptAlgorithm.LN_BOBYQA, x0, out x1, out obj1, out exit1);

                if (ok1 && obj1 > bestObj)
                {
                    bestObj = obj1;
                    bestX = x1;
                    bestExit = exit1;
                }

                // Fallback to COBYLA if BOBYQA fails
                if (!ok1)
                {
                    if (_logger != null)
                        _logger.LogDebug("BOBYQA failed on start {0}; trying COBYLA fallback.", i);

                    double[] x2;
                    double obj2;
                    NloptResult exit2;
                    bool ok2 = TryLocalMaximize(NLoptAlgorithm.LN_COBYLA, x0, out x2, out obj2, out exit2);

                    if (ok2 && obj2 > bestObj)
                    {
                        bestObj = obj2;
                        bestX = x2;
                        bestExit = exit2;
                    }
                }
            }

            if (bestX == null)
            {
                return new OptimizationResult(
                    new double[0],
                    double.NaN,
                    OptimizerExitStatus.Failure);
            }

            return new OptimizationResult(
                bestX,
                bestObj,
                NloptResultMapper.MapToExitStatus(bestExit));
        }

        private bool TryLocalMaximize(
            NLoptAlgorithm algorithm,
            double[] x0,
            out double[] bestX,
            out double bestObjective,
            out NloptResult exitCode)
        {
            bestX = (double[])x0.Clone();
            bestObjective = double.NegativeInfinity;
            exitCode = NloptResult.FAILURE;

            NLoptSolver opt = null;

            try
            {
                uint n = (uint)_lowerBounds.Length;

                // Use the ctor for tolerances/iterations per your wrapper
                opt = new NLoptSolver(
                    algorithm,
                    n,
                    _relativeStoppingTolerance,
                    _maximumIterations,
                    null);

                opt.SetLowerBounds(_lowerBounds);
                opt.SetUpperBounds(_upperBounds);

                // Minimize -f(x) => maximize f(x)
                opt.SetMinObjective(ObjectiveAdapterMinimizingNegative);

                double[] x = (double[])bestX.Clone();

                exitCode = opt.Optimize(x, out double? minf);
                
                double? obj = -minf;
                if (obj.HasValue)
                {

                    if (double.IsNaN(obj.Value) || double.IsInfinity(obj.Value))
                        return false;

                    bestX = x;
                    bestObjective = obj.Value;

                    return IsAcceptableExit(exitCode);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogDebug(ex, "NLopt {0} threw during optimization.", algorithm);
                return false;
            }
            finally
            {
                if (opt != null)
                    opt.Dispose();
            }
        }

        // Common NLoptNet signature: double f(double[] x, double[] grad)
        private double ObjectiveAdapterMinimizingNegative(double[] x, double[] grad)
        {
            double val = _pinnedObjectiveFunction(x);

            // If objective invalid, punish it
            if (double.IsNaN(val) || double.IsInfinity(val))
                return double.PositiveInfinity;

            return -val;
        }

        private bool IsAcceptableExit(NloptResult r)
        {
            return r == NloptResult.SUCCESS
                || r == NloptResult.STOPVAL_REACHED
                || r == NloptResult.FTOL_REACHED
                || r == NloptResult.XTOL_REACHED
                || r == NloptResult.MAXEVAL_REACHED
                || r == NloptResult.MAXTIME_REACHED;
        }

        private double[] SampleUniformWithinBounds()
        {
            int n = _lowerBounds.Length;
            var x = new double[n];

            for (int i = 0; i < n; i++)
            {
                double lo = _lowerBounds[i];
                double hi = _upperBounds[i];
                if (hi < lo) throw new ArgumentException("Upper bound < lower bound at dim " + i);

                x[i] = lo + (hi - lo) * _rng.NextDouble();
            }

            return x;
        }

        private void ValidateInputs(double[] initialGuess)
        {
            if (_objectiveFunction == null)
                throw new InvalidOperationException("Objective function must be set.");

            if (_lowerBounds == null || _upperBounds == null)
                throw new InvalidOperationException("Bounds must be set.");

            if (initialGuess == null)
                throw new ArgumentNullException(nameof(initialGuess));

            if (initialGuess.Length != _lowerBounds.Length || initialGuess.Length != _upperBounds.Length)
                throw new ArgumentException("Initial guess must match number of dimensions.");

            for (int i = 0; i < initialGuess.Length; i++)
            {
                if (_upperBounds[i] < _lowerBounds[i])
                    throw new ArgumentException("Upper bound < lower bound at dim " + i);
            }
        }
    }

}