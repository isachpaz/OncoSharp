// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Statistics.Abstractions.Interfaces;
using OncoSharp.Statistics.Abstractions.MLEEstimators;

namespace OncoSharp.Statistics.Abstractions.ConfidenceInterval
{
    public class ProfileLikelihoodCI<TData, TParameters> where TParameters : new()
    {
        private readonly IMleInternals<TData, TParameters> _mle;
        private readonly IList<TData> _inputData;
        private readonly IList<bool> _observations;
        private readonly double _deltaLogL;
        private readonly int _maxIterations;

        public ProfileLikelihoodCI(
            MaximumLikelihoodEstimator<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            double confidenceLevel = 0.95,
            int maxIterations = 100)
        {
            _mle = (IMleInternals<TData, TParameters>)estimator;
            _inputData = inputData;
            _observations = observations;
            _deltaLogL = ChiSquaredDelta(confidenceLevel); // e.g., 1.92 for 95%
            _maxIterations = maxIterations;
        }

        public (double lower, double upper) GetConfidenceInterval(
            int paramIndex,
            double mleValue,
            double[] mleParams)
        {
            var lower1 = _mle.GetLowerBounds()[paramIndex];
            var upper1 = _mle.GetUpperBounds()[paramIndex];
            if (Math.Abs(upper1 - lower1) < 1e12)
            {
                return (lower1, upper1);
            }

            double bestLogL = _mle.LogLikelihood(
                _mle.ConvertVectorToParameters(mleParams),
                _observations,
                _inputData);

            double targetLogL = bestLogL - _deltaLogL;

            // Lower bound search
            double lower = FindBound(
                direction: -1,
                startValue: mleValue,
                paramIndex: paramIndex,
                targetLogL: targetLogL,
                mleParams: mleParams);

            // Upper bound search
            double upper = FindBound(
                direction: 1,
                startValue: mleValue,
                paramIndex: paramIndex,
                targetLogL: targetLogL,
                mleParams: mleParams);

            return (lower, upper);
        }

        //private double FindBound(
        //    int direction,
        //    double startValue,
        //    int paramIndex,
        //    double targetLogL,
        //    double[] mleParams)
        //{
        //    double step = 0.05 * Math.Abs(startValue == 0 ? 1.0 : startValue);
        //    double current = startValue;
        //    double previous = current;
        //    int iter = 0;

        //    while (iter++ < _maxIterations)
        //    {
        //        current += direction * step;

        //        double[] testParams = (double[])mleParams.Clone();
        //        testParams[paramIndex] = current;

        //        double logL = MaximizeWithFixedParameter(testParams, paramIndex);

        //        if (logL < targetLogL)
        //        {
        //            // Use bisection to refine
        //            return BisectionSearch(paramIndex, previous, current, targetLogL, mleParams,  maxIter: _maxIterations);
        //        }

        //        previous = current;
        //    }

        //    if (direction == -1)
        //    {
        //        return double.NegativeInfinity;
        //    }
        //    else
        //    {
        //        return double.PositiveInfinity;
        //    }

        //    throw new InvalidOperationException("Profile likelihood CI bound not found within iteration limit.");
        //}

        private double FindBound(
            int direction,
            double startValue,
            int paramIndex,
            double targetLogL,
            double[] mleParams)
        {
            try
            {
                var (a, b) = BracketLogLikelihoodDrop(direction, startValue, paramIndex, targetLogL, mleParams);
                return BisectionSearch(paramIndex, a, b, targetLogL, mleParams, tol: 1e-6, maxIter: _maxIterations);
            }
            catch (InvalidOperationException)
            {
                return direction == -1 ? double.NegativeInfinity : double.PositiveInfinity;
            }
        }


        private (double a, double b) BracketLogLikelihoodDrop(
            int direction,
            double startValue,
            int paramIndex,
            double targetLogL,
            double[] mleParams,
            double initialStep = 0.01,
            int maxGrowthSteps = 20)
        {
            double factor = 1.5;
            double step = initialStep * Math.Abs(startValue == 0 ? 1.0 : startValue);
            double current = startValue;
            double previous = current;

            for (int i = 0; i < maxGrowthSteps; i++)
            {
                current += direction * step;

                double[] testParams = (double[])mleParams.Clone();
                testParams[paramIndex] = current;
                double logL = MaximizeWithFixedParameter(testParams, paramIndex);

                if (logL < targetLogL)
                {
                    return direction == -1 ? (current, previous) : (previous, current);
                }

                previous = current;
                step *= factor;
            }

            throw new InvalidOperationException("Could not bracket likelihood drop.");
        }


        private double MaximizeWithFixedParameter(double[] fixedParams, int fixedIndex)
        {
            int paramCount = fixedParams.Length;
            var optimizer = _mle.CreateSolver(paramCount - 1);

            double[] lower = _mle.GetLowerBounds() ?? Enumerable.Repeat(double.NegativeInfinity, paramCount).ToArray();
            double[] upper = _mle.GetUpperBounds() ?? Enumerable.Repeat(double.PositiveInfinity, paramCount).ToArray();

            // Remove fixed parameter dimension
            double[] start = fixedParams.Where((_, i) => i != fixedIndex).ToArray();
            double[] lowerBound = lower.Where((_, i) => i != fixedIndex).ToArray();
            double[] upperBound = upper.Where((_, i) => i != fixedIndex).ToArray();

            optimizer
                .SetLowerBounds(lowerBound)
                .SetUpperBounds(upperBound)
                .SetMaxObjective(varying =>
                {
                    double[] full = InsertFixedParameter(fixedParams, varying, fixedIndex);
                    var paramObj = _mle.ConvertVectorToParameters(full);
                    return _mle.LogLikelihood(paramObj, _observations, _inputData);
                });

            var result = optimizer.MaximizeFromSingleStart(start);

            return result.ObjectiveValue;
        }

        private static double[] InsertFixedParameter(double[] fixed1, double[] varying, int fixedIndex)
        {
            double[] full = new double[fixed1.Length];
            int vi = 0;
            for (int i = 0; i < full.Length; i++)
            {
                full[i] = i == fixedIndex ? fixed1[i] : varying[vi++];
            }

            return full;
        }

        private double BisectionSearch(
            int paramIndex,
            double low,
            double high,
            double targetLogL,
            double[] mleParams,
            double tol = 1e-6,
            int maxIter = 50)
        {
            for (int i = 0; i < maxIter; i++)
            {
                double mid = (low + high) / 2;
                double[] testParams = (double[])mleParams.Clone();
                testParams[paramIndex] = mid;

                double logL = MaximizeWithFixedParameter(testParams, paramIndex);

                if (Math.Abs(logL - targetLogL) < tol)
                    return mid;

                if (logL > targetLogL)
                    low = mid;
                else
                    high = mid;
            }

            return (low + high) / 2;
        }

        private static double ChiSquaredDelta(double confidenceLevel)
        {
            // Inverse CDF of chi^2(1) at given confidence level
            // 0.95 => ~3.84, delta = 0.5 * chi^2 = 1.92
            switch (confidenceLevel)
            {
                case 0.95:
                    return 1.92;
                case 0.90:
                    return 1.35;
                default:
                    throw new NotSupportedException("Unsupported confidence level.");
            }
        }
    }
}