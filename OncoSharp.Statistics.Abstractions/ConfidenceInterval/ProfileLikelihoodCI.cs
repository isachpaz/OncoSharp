// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NLoptNet;
using OncoSharp.Optimization.Algorithms.NLopt;
using OncoSharp.Statistics.Abstractions.Helpers;
using OncoSharp.Statistics.Abstractions.Interfaces;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using MathNet.Numerics.Distributions;
using Plotly.NET;
using Plotly.NET.TraceObjects;

namespace OncoSharp.Statistics.Abstractions.ConfidenceInterval
{
    public class ProfileLikelihoodCI<TData, TParameters> where TParameters : new()
    {
        private readonly IMleInternals<TData, TParameters> _mle;
        private readonly IList<TData> _inputData;
        private readonly IList<bool> _observations;
        private readonly double _deltaLogL;
        private readonly int _maxIterations;
        private readonly double _initialStepSize;
        private readonly double _stepGrowthFactor;
        private readonly double _tolerance;
        private double[] _lastMleParameters;
        private readonly Dictionary<int, double> _lastMleValues = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _lastMleLogLs = new Dictionary<int, double>();
        private readonly Dictionary<int, double> _lastTargetLogLs = new Dictionary<int, double>();
        private readonly Dictionary<int, List<(double value, double logL)>> _evaluationCache =
            new Dictionary<int, List<(double value, double logL)>>();

        // Diagnostic and visualization properties
        public bool EnableDiagnostics { get; set; } = false;
        public Dictionary<int, List<(double value, double logL)>> ProfileHistories { get; } =
            new Dictionary<int, List<(double, double)>>();
        public Dictionary<int, string> ParameterNames { get; } = new Dictionary<int, string>();
        public Dictionary<int, double> ParameterStepSizes { get; } = new Dictionary<int, double>();

        public ProfileLikelihoodCI(
            MaximumLikelihoodEstimator<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            double confidenceLevel = 0.95,
            int maxIterations = 100,
            double initialStepSize = 0.01,
            double stepGrowthFactor = 1.5,
            double tolerance = 1e-6)
        {
            _mle = (IMleInternals<TData, TParameters>)estimator ?? throw new ArgumentNullException(nameof(estimator));
            _inputData = inputData ?? throw new ArgumentNullException(nameof(inputData));
            _observations = observations ?? throw new ArgumentNullException(nameof(observations));

            if (confidenceLevel <= 0 || confidenceLevel >= 1)
                throw new ArgumentOutOfRangeException(nameof(confidenceLevel), "Confidence level must be between 0 and 1");

            _deltaLogL = ChiSquaredDelta(confidenceLevel);
            _maxIterations = maxIterations;
            _initialStepSize = initialStepSize;
            _stepGrowthFactor = stepGrowthFactor;
            _tolerance = tolerance;
        }

        public void SetParameterName(int paramIndex, string paramName)
        {
            ParameterNames[paramIndex] = paramName;
        }

        public void SetParameterStepSize(int paramIndex, double stepSize)
        {
            ParameterStepSizes[paramIndex] = stepSize;
        }

        public (double lower, double upper) GetConfidenceInterval(
            int paramIndex,
            double mleValue,
            double[] mleParams)
        {
            if (mleParams == null)
                throw new ArgumentNullException(nameof(mleParams));
            if (paramIndex < 0 || paramIndex >= mleParams.Length)
                throw new ArgumentOutOfRangeException(nameof(paramIndex));

            var lowerBound = _mle.GetLowerBounds()[paramIndex];
            var upperBound = _mle.GetUpperBounds()[paramIndex];

            // Initialize profile history for this parameter
            if (EnableDiagnostics)
            {
                if (!ProfileHistories.ContainsKey(paramIndex))
                {
                    ProfileHistories[paramIndex] = new List<(double, double)>();
                }
                else
                {
                    ProfileHistories[paramIndex].Clear();
                }
            }

            // Handle fixed parameter case
            if (Math.Abs(upperBound - lowerBound) < 1e-12)
            {
                return (lowerBound, upperBound);
            }

            // Ensure MLE value is within bounds
            mleValue = MathUtils.Clamp(mleValue, lowerBound, upperBound);

            // Keep parameters consistent with the MLE value used for profiling
            double[] profileParams = (double[])mleParams.Clone();
            profileParams[paramIndex] = mleValue;
            _lastMleParameters = (double[])profileParams.Clone();
            ResetEvaluationCache(paramIndex);

            double bestLogL = EvaluateLogLikelihood(mleValue, paramIndex, profileParams);
            if (!IsFinite(bestLogL))
            {
                bestLogL = _mle.LogLikelihood(
                    _mle.ConvertVectorToParameters(profileParams),
                    _observations,
                    _inputData);
            }

            // Record MLE point if diagnostics are enabled
            if (EnableDiagnostics)
            {
                ProfileHistories[paramIndex].Add((mleValue, bestLogL));
            }

            double targetLogL = bestLogL - _deltaLogL;
            _lastMleValues[paramIndex] = mleValue;
            _lastMleLogLs[paramIndex] = bestLogL;
            _lastTargetLogLs[paramIndex] = targetLogL;

            // Lower bound search
            double lower = FindBound(
                direction: -1,
                startValue: mleValue,
                paramIndex: paramIndex,
                targetLogL: targetLogL,
                mleParams: profileParams);

            // Upper bound search
            double upper = FindBound(
                direction: 1,
                startValue: mleValue,
                paramIndex: paramIndex,
                targetLogL: targetLogL,
                mleParams: profileParams);

            if (EnableDiagnostics)
            {
                AddBoundaryPointsIfNeeded(paramIndex, lowerBound, upperBound, profileParams);
                if (TryGetCiFromHistory(ProfileHistories[paramIndex], targetLogL, mleValue, out double? histLower, out double? histUpper))
                {
                    if (histLower.HasValue)
                    {
                        if (Math.Abs(lower - histLower.Value) > _tolerance * 10)
                        {
                            Debug.WriteLine(
                                $"ProfileLikelihoodCI paramIndex={paramIndex} lower CI override from history: {lower:G6} -> {histLower.Value:G6}");
                        }
                        lower = histLower.Value;
                    }

                    if (histUpper.HasValue)
                    {
                        if (Math.Abs(upper - histUpper.Value) > _tolerance * 10)
                        {
                            Debug.WriteLine(
                                $"ProfileLikelihoodCI paramIndex={paramIndex} upper CI override from history: {upper:G6} -> {histUpper.Value:G6}");
                        }
                        upper = histUpper.Value;
                    }
                }
                AddCiPointsIfNeeded(paramIndex, lower, upper, profileParams);
            }

            return (lower, upper);
        }

        private double FindBound(
            int direction,
            double startValue,
            int paramIndex,
            double targetLogL,
            double[] mleParams)
        {
            var lowerBounds = _mle.GetLowerBounds();
            var upperBounds = _mle.GetUpperBounds();
            double paramLowerBound = lowerBounds[paramIndex];
            double paramUpperBound = upperBounds[paramIndex];

            if (!TryBracketLogLikelihoodDrop(
                    direction,
                    startValue,
                    paramIndex,
                    targetLogL,
                    mleParams,
                    paramLowerBound,
                    paramUpperBound,
                    out double a,
                    out double b,
                    out var history))
            {
                if (TryFindCrossingFromHistory(history, targetLogL, startValue, direction, out double fallback))
                {
                    return fallback;
                }

                return direction == -1 ? paramLowerBound : paramUpperBound;
            }

            a = MathUtils.Clamp(a, paramLowerBound, paramUpperBound);
            b = MathUtils.Clamp(b, paramLowerBound, paramUpperBound);

            double fa = EvaluateLogLikelihood(a, paramIndex, mleParams) - targetLogL;
            double fb = EvaluateLogLikelihood(b, paramIndex, mleParams) - targetLogL;

            if (!IsFinite(fa) || !IsFinite(fb))
            {
                if (TryFindCrossingFromHistory(history, targetLogL, startValue, direction, out double fallback))
                {
                    return fallback;
                }

                return direction == -1 ? paramLowerBound : paramUpperBound;
            }

            if (TryBobyqaRefinement(paramIndex, a, b, targetLogL, mleParams, out double refined))
            {
                double fr = EvaluateLogLikelihood(refined, paramIndex, mleParams) - targetLogL;
                if (IsFinite(fr))
                {
                    if (Math.Abs(fr) < _tolerance)
                    {
                        return refined;
                    }

                    if (fa * fr <= 0)
                    {
                        b = refined;
                        fb = fr;
                    }
                    else if (fr * fb <= 0)
                    {
                        a = refined;
                        fa = fr;
                    }
                }
            }

            try
            {
                return BisectionSearch(
                    paramIndex,
                    a,
                    b,
                    fa,
                    fb,
                    targetLogL,
                    mleParams,
                    paramLowerBound,
                    paramUpperBound,
                    _maxIterations);
            }
            catch (InvalidOperationException)
            {
                if (TryFindCrossingFromHistory(history, targetLogL, startValue, direction, out double fallback))
                {
                    return fallback;
                }

                // If we can't bracket before hitting a bound, the CI hits the bound
                return direction == -1 ? paramLowerBound : paramUpperBound;
            }
        }

        private bool TryBracketLogLikelihoodDrop(
            int direction,
            double startValue,
            int paramIndex,
            double targetLogL,
            double[] mleParams,
            double lowerFixed,
            double upperFixed,
            out double a,
            out double b,
            out List<(double value, double logL)> history)
        {
            a = double.NaN;
            b = double.NaN;

            // Use parameter-specific step size if available
            double baseStep = ParameterStepSizes.TryGetValue(paramIndex, out double specificStep)
                ? specificStep
                : _initialStepSize * (Math.Abs(startValue) < 1e-10 ? 1.0 : Math.Abs(startValue));

            double step = baseStep;
            double current = startValue;
            double previous = current;
            double previousLogL = double.NaN;

            // Keep a local history for adaptive stepping; optionally mirror to diagnostics.
            history = new List<(double value, double logL)>();
            List<(double value, double logL)> diagnosticHistory = null;
            if (EnableDiagnostics)
            {
                if (!ProfileHistories.TryGetValue(paramIndex, out diagnosticHistory))
                {
                    diagnosticHistory = new List<(double value, double logL)>();
                    ProfileHistories[paramIndex] = diagnosticHistory;
                }
            }

            // Add starting point to history
            double startLogL = MaximizeWithFixedParameter(CreateTestParams(mleParams, paramIndex, current), paramIndex);
            if (!IsFinite(startLogL))
            {
                startLogL = double.NegativeInfinity;
            }
            history.Add((current, startLogL));
            if (diagnosticHistory != null)
            {
                diagnosticHistory.Add((current, startLogL));
            }

            // Track the best log-likelihood found so far
            double bestLogL = startLogL;

            for (int i = 0; i < _maxIterations; i++)
            {
                // Adaptive step sizing based on multiple factors
                if (history.Count >= 3)
                {
                    // Use the last 3 points for curvature estimation
                    var (x1, y1) = history[history.Count - 3];
                    var (x2, y2) = history[history.Count - 2];
                    var (x3, y3) = history[history.Count - 1];

                    if (IsFinite(x1) && IsFinite(x2) && IsFinite(x3) &&
                        IsFinite(y1) && IsFinite(y2) && IsFinite(y3))
                    {
                        // Calculate second derivative approximation (curvature)
                        double h1 = x2 - x1;
                        double h2 = x3 - x2;
                        double h12 = h1 + h2;

                        if (Math.Abs(h1) > _tolerance && Math.Abs(h2) > _tolerance && Math.Abs(h12) > _tolerance)
                        {
                            double y12 = (y2 - y1) / h1;
                            double y23 = (y3 - y2) / h2;
                            double secondDeriv = 2 * (y23 - y12) / h12;

                            // Calculate first derivative approximation (gradient)
                            double gradient = (y3 - y2) / h2;

                            // Calculate distance to target
                            double distanceToTarget = Math.Abs(y3 - targetLogL);

                            if (IsFinite(secondDeriv) && IsFinite(gradient) && IsFinite(distanceToTarget))
                            {
                                // Calculate adaptive factors
                                double curvatureFactor = 1.0 / (1.0 + Math.Abs(secondDeriv));
                                double gradientFactor = 1.0 / (1.0 + Math.Abs(gradient));
                                double targetFactor = 1.0 / (1.0 + distanceToTarget);

                                // Combine factors with weights
                                double combinedFactor = 0.5 * curvatureFactor + 0.3 * gradientFactor + 0.2 * targetFactor;

                                if (IsFinite(combinedFactor) && combinedFactor > 0)
                                {
                                    // Adjust step size
                                    step = baseStep * combinedFactor;

                                    // Ensure step doesn't get too small or too large
                                    step = Math.Max(baseStep * 0.01, Math.Min(step, baseStep * 10));
                                }
                            }
                        }
                    }
                }

                if (!IsFinite(step) || step <= 0)
                {
                    step = baseStep;
                }

                // Apply direction and step
                current += direction * step;

                // Clamp to bounds
                current = MathUtils.Clamp(current, lowerFixed, upperFixed);

                // Evaluate log-likelihood at current point
                double[] testParams = (double[])mleParams.Clone();
                testParams[paramIndex] = current;
                double logL = MaximizeWithFixedParameter(testParams, paramIndex);
                if (!IsFinite(logL))
                {
                    logL = double.NegativeInfinity;
                }

                // Store this point for future adaptive step calculations
                history.Add((current, logL));
                if (diagnosticHistory != null)
                {
                    diagnosticHistory.Add((current, logL));
                }

                // Update best log-likelihood
                if (logL > bestLogL)
                {
                    bestLogL = logL;
                }

                // Check if we've crossed the target
                if (logL < targetLogL)
                {
                    if (double.IsNaN(previousLogL))
                    {
                        // We need at least two points to bracket
                        previousLogL = MaximizeWithFixedParameter(
                            CreateTestParams(mleParams, paramIndex, previous),
                            paramIndex);
                    }
                    if (direction == -1)
                    {
                        a = current;
                        b = previous;
                    }
                    else
                    {
                        a = previous;
                        b = current;
                    }
                    return true;
                }

                // If we've hit a bound and still haven't dropped, we can't go further
                if ((direction == -1 && current <= lowerFixed) ||
                    (direction == 1 && current >= upperFixed))
                {
                    return false;
                }

                // Check if we're making progress
                if (Math.Abs(current - previous) < _tolerance * 0.1)
                {
                    // We're not making meaningful progress, increase step size
                    step *= 2;
                }

                // Prepare for next iteration
                previous = current;
                previousLogL = logL;

                // Apply growth factor to step size for next iteration
                step *= _stepGrowthFactor;
            }

            // Fallback: evaluate at bound to see if we can still bracket.
            double boundValue = direction == -1 ? lowerFixed : upperFixed;
            if (boundValue != current)
            {
                double boundLogL = MaximizeWithFixedParameter(
                    CreateTestParams(mleParams, paramIndex, boundValue),
                    paramIndex);
                if (!IsFinite(boundLogL))
                {
                    boundLogL = double.NegativeInfinity;
                }

                history.Add((boundValue, boundLogL));
                if (diagnosticHistory != null)
                {
                    diagnosticHistory.Add((boundValue, boundLogL));
                }

                if (boundLogL < targetLogL)
                {
                    if (direction == -1)
                    {
                        a = boundValue;
                        b = current;
                    }
                    else
                    {
                        a = current;
                        b = boundValue;
                    }
                    return true;
                }
            }

            return false;
        }

        private double MaximizeWithFixedParameter(double[] fixedParams, int fixedIndex)
        {
            int paramCount = fixedParams.Length;

            // Special case: only one parameter in the model, and it's fixed
            if (paramCount == 1)
            {
                var paramObjSingle = _mle.ConvertVectorToParameters(fixedParams);
                return _mle.LogLikelihood(paramObjSingle, _observations, _inputData);
            }

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

        private bool TryBobyqaRefinement(
            int paramIndex,
            double a,
            double b,
            double targetLogL,
            double[] mleParams,
            out double refined)
        {
            refined = double.NaN;

            if (!IsFinite(a) || !IsFinite(b) || b <= a)
                return false;

            double width = b - a;
            if (width < _tolerance)
                return false;

            double[] lower = { a };
            double[] upper = { b };
            double[] start = { a + 0.5 * width };

            int maxEval = Math.Max(20, _maxIterations);
            double xtolRel = Math.Max(_tolerance, 1e-8);

            try
            {
                var optimizer = new NLoptOptimizer(NLoptAlgorithm.LN_BOBYQA, 1, xtolRel, maxEval);
                optimizer
                    .SetLowerBounds(lower)
                    .SetUpperBounds(upper)
                    .SetMaxObjective(x =>
                    {
                        double logL = EvaluateLogLikelihood(x[0], paramIndex, mleParams);
                        if (!IsFinite(logL))
                            return double.NegativeInfinity;

                        double diff = logL - targetLogL;
                        return -(diff * diff);
                    });

                var result = optimizer.MaximizeFromSingleStart(start);
                if (result == null || result.OptimizedParameters == null || result.OptimizedParameters.Length == 0)
                    return false;

                refined = MathUtils.Clamp(result.OptimizedParameters[0], a, b);
                return IsFinite(refined);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private double BisectionSearch(
            int paramIndex,
            double a,
            double b,
            double fa,
            double fb,
            double targetLogL,
            double[] mleParams,
            double lowerFixed,
            double upperFixed,
            int maxIter)
        {
            if (a > b)
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
            }

            a = MathUtils.Clamp(a, lowerFixed, upperFixed);
            b = MathUtils.Clamp(b, lowerFixed, upperFixed);

            if (!IsFinite(fa))
                fa = EvaluateLogLikelihood(a, paramIndex, mleParams) - targetLogL;
            if (!IsFinite(fb))
                fb = EvaluateLogLikelihood(b, paramIndex, mleParams) - targetLogL;

            if (!IsFinite(fa) || !IsFinite(fb))
                throw new InvalidOperationException("Non-finite log-likelihood encountered in bisection endpoints.");

            if (Math.Abs(fa) < _tolerance) return a;
            if (Math.Abs(fb) < _tolerance) return b;

            if (fa * fb > 0)
                throw new InvalidOperationException("No root in the given interval");

            if (maxIter <= 0) maxIter = 50;

            for (int iter = 0; iter < maxIter; iter++)
            {
                double mid = 0.5 * (a + b);
                double fm = EvaluateLogLikelihood(mid, paramIndex, mleParams) - targetLogL;

                if (!IsFinite(fm))
                    throw new InvalidOperationException("Non-finite log-likelihood encountered during bisection.");

                if (Math.Abs(fm) < _tolerance || Math.Abs(b - a) < _tolerance)
                    return mid;

                if (fa * fm > 0)
                {
                    a = mid;
                    fa = fm;
                }
                else
                {
                    b = mid;
                    fb = fm;
                }
            }

            return 0.5 * (a + b);
        }

        private bool TryFindCrossingFromHistory(
            List<(double value, double logL)> history,
            double targetLogL,
            double startValue,
            int direction,
            out double crossing)
        {
            crossing = double.NaN;

            if (history == null || history.Count < 2)
                return false;

            var finite = history
                .Where(p => IsFinite(p.value) && IsFinite(p.logL))
                .OrderBy(p => p.value)
                .ToList();

            if (finite.Count < 2)
                return false;

            double tol = _tolerance * Math.Max(1.0, Math.Abs(startValue));

            if (direction < 0)
            {
                var left = finite.Where(p => p.value <= startValue + tol).ToList();
                if (left.Count < 2)
                    return false;

                for (int i = left.Count - 1; i > 0; i--)
                {
                    var p2 = left[i];
                    var p1 = left[i - 1];
                    double y2 = p2.logL - targetLogL;
                    double y1 = p1.logL - targetLogL;

                    if (Math.Abs(y2) < _tolerance)
                    {
                        crossing = p2.value;
                        return true;
                    }

                    if (Math.Abs(y1) < _tolerance)
                    {
                        crossing = p1.value;
                        return true;
                    }

                    if (y1 * y2 < 0)
                    {
                        crossing = Interpolate(p1.value, p1.logL, p2.value, p2.logL, targetLogL);
                        return IsFinite(crossing);
                    }
                }
            }
            else
            {
                var right = finite.Where(p => p.value >= startValue - tol).ToList();
                if (right.Count < 2)
                    return false;

                for (int i = 0; i < right.Count - 1; i++)
                {
                    var p1 = right[i];
                    var p2 = right[i + 1];
                    double y1 = p1.logL - targetLogL;
                    double y2 = p2.logL - targetLogL;

                    if (Math.Abs(y1) < _tolerance)
                    {
                        crossing = p1.value;
                        return true;
                    }

                    if (Math.Abs(y2) < _tolerance)
                    {
                        crossing = p2.value;
                        return true;
                    }

                    if (y1 * y2 < 0)
                    {
                        crossing = Interpolate(p1.value, p1.logL, p2.value, p2.logL, targetLogL);
                        return IsFinite(crossing);
                    }
                }
            }

            return false;
        }

        private double BrentSearch(
            int paramIndex,
            double a,
            double b,
            double targetLogL,
            double[] mleParams,
            double lowerFixed,
            double upperFixed,
            int maxIter = 50)
        {
            // Ensure a and b are ordered correctly
            if (a > b) (a, b) = (b, a);

            // Clamp endpoints to bounds
            a = MathUtils.Clamp(a, lowerFixed, upperFixed);
            b = MathUtils.Clamp(b, lowerFixed, upperFixed);

            double fa = EvaluateLogLikelihood(a, paramIndex, mleParams) - targetLogL;
            double fb = EvaluateLogLikelihood(b, paramIndex, mleParams) - targetLogL;

            // Check if we already have the solution at one of the endpoints
            if (Math.Abs(fa) < _tolerance) return a;
            if (Math.Abs(fb) < _tolerance) return b;

            // Ensure we have a root between a and b
            if (fa * fb > 0)
                throw new InvalidOperationException("No root in the given interval");

            double c = a;
            double fc = fa;
            double d = b - a;
            double e = d;

            for (int iter = 0; iter < maxIter; iter++)
            {
                if (Math.Abs(fc) < Math.Abs(fb))
                {
                    // Swap to make sure b is the best approximation
                    (a, b, c) = (b, c, a);
                    (fa, fb, fc) = (fb, fc, fa);
                }

                double tol = 2.0 * _tolerance * Math.Abs(b) + _tolerance / 2.0;
                double m = 0.5 * (c - b);

                if (Math.Abs(m) <= tol || Math.Abs(fb) < _tolerance)
                    return b;

                // Try inverse quadratic interpolation
                if (Math.Abs(e) >= tol && Math.Abs(fa) > Math.Abs(fb))
                {
                    double s = fb / fa;
                    double p, q;

                    if (a == c)
                    {
                        p = 2.0 * m * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fa / fc;
                        double r = fb / fc;
                        p = s * (2.0 * m * q * (q - r) - (b - a) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }

                    if (p > 0) q = -q;
                    p = Math.Abs(p);

                    if (2.0 * p < Math.Min(3.0 * m * q - Math.Abs(tol * q), Math.Abs(e * q)))
                    {
                        e = d;
                        d = p / q;
                        continue;
                    }
                }

                // Fall back to bisection
                e = d = m;
            }

            return b; // Return best approximation
        }

        private double EvaluateLogLikelihood(double value, int paramIndex, double[] mleParams)
        {
            if (TryGetCachedLogL(paramIndex, value, out double cached))
            {
                return cached;
            }

            double[] testParams = (double[])mleParams.Clone();
            testParams[paramIndex] = value;
            double logL = MaximizeWithFixedParameter(testParams, paramIndex);
            if (IsFinite(logL))
            {
                CacheLogL(paramIndex, value, logL);
                return logL;
            }

            return double.NegativeInfinity;
        }

        private double[] CreateTestParams(double[] mleParams, int paramIndex, double value)
        {
            double[] testParams = (double[])mleParams.Clone();
            testParams[paramIndex] = value;
            return testParams;
        }

        private static double[] InsertFixedParameter(double[] fixedParams, double[] varying, int fixedIndex)
        {
            double[] full = new double[fixedParams.Length];
            int varyingIndex = 0;

            for (int i = 0; i < full.Length; i++)
            {
                full[i] = i == fixedIndex ? fixedParams[i] : varying[varyingIndex++];
            }

            return full;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private void ResetEvaluationCache(int paramIndex)
        {
            if (_evaluationCache.TryGetValue(paramIndex, out var cache))
            {
                cache.Clear();
            }
            else
            {
                _evaluationCache[paramIndex] = new List<(double value, double logL)>();
            }
        }

        private bool TryGetCachedLogL(int paramIndex, double value, out double logL)
        {
            logL = double.NaN;
            if (!_evaluationCache.TryGetValue(paramIndex, out var cache) || cache.Count == 0)
                return false;

            double tol = _tolerance * Math.Max(1.0, Math.Abs(value));
            foreach (var point in cache)
            {
                if (Math.Abs(point.value - value) <= tol)
                {
                    logL = point.logL;
                    return true;
                }
            }

            return false;
        }

        private void CacheLogL(int paramIndex, double value, double logL)
        {
            if (!IsFinite(value) || !IsFinite(logL))
                return;

            if (!_evaluationCache.TryGetValue(paramIndex, out var cache))
            {
                cache = new List<(double value, double logL)>();
                _evaluationCache[paramIndex] = cache;
            }

            double tol = _tolerance * Math.Max(1.0, Math.Abs(value));
            foreach (var point in cache)
            {
                if (Math.Abs(point.value - value) <= tol)
                {
                    return;
                }
            }

            cache.Add((value, logL));
        }

        private bool TryGetCiFromHistory(
            List<(double value, double logL)> history,
            double targetLogL,
            double mleValue,
            out double? ciLower,
            out double? ciUpper)
        {
            ciLower = null;
            ciUpper = null;

            if (history == null || history.Count < 2)
                return false;

            var sortedHistory = history
                .Where(p => IsFinite(p.value) && IsFinite(p.logL))
                .OrderBy(p => p.value)
                .ToArray();

            if (sortedHistory.Length < 2)
                return false;

            var crossings = new List<double>();
            for (int i = 0; i < sortedHistory.Length - 1; i++)
            {
                double x1 = sortedHistory[i].value;
                double y1 = sortedHistory[i].logL - targetLogL;
                double x2 = sortedHistory[i + 1].value;
                double y2 = sortedHistory[i + 1].logL - targetLogL;

                if (!IsFinite(y1) || !IsFinite(y2))
                    continue;

                if (Math.Abs(y1) < _tolerance)
                    crossings.Add(x1);
                if (Math.Abs(y2) < _tolerance)
                    crossings.Add(x2);

                if (y1 * y2 < 0)
                {
                    double cross = Interpolate(sortedHistory[i].value, sortedHistory[i].logL,
                                               sortedHistory[i + 1].value, sortedHistory[i + 1].logL,
                                               targetLogL);
                    if (IsFinite(cross))
                    {
                        crossings.Add(cross);
                    }
                }
            }

            if (crossings.Count == 0)
                return false;

            double tol = _tolerance * Math.Max(1.0, Math.Abs(mleValue));
            var left = crossings.Where(value => value <= mleValue + tol).ToList();
            var right = crossings.Where(value => value >= mleValue - tol).ToList();

            if (left.Count > 0)
            {
                ciLower = left.Max();
            }

            if (right.Count > 0)
            {
                ciUpper = right.Min();
            }

            return ciLower.HasValue || ciUpper.HasValue;
        }

        private void AddBoundaryPointsIfNeeded(
            int paramIndex,
            double lowerBound,
            double upperBound,
            double[] mleParams)
        {
            if (!EnableDiagnostics)
                return;

            if (!ProfileHistories.TryGetValue(paramIndex, out var history))
            {
                history = new List<(double value, double logL)>();
                ProfileHistories[paramIndex] = history;
            }

            TryAddHistoryPoint(history, paramIndex, lowerBound, mleParams);
            TryAddHistoryPoint(history, paramIndex, upperBound, mleParams);
        }

        private void AddCiPointsIfNeeded(
            int paramIndex,
            double lower,
            double upper,
            double[] mleParams)
        {
            if (!EnableDiagnostics)
                return;

            if (!ProfileHistories.TryGetValue(paramIndex, out var history))
            {
                history = new List<(double value, double logL)>();
                ProfileHistories[paramIndex] = history;
            }

            TryAddHistoryPoint(history, paramIndex, lower, mleParams);
            TryAddHistoryPoint(history, paramIndex, upper, mleParams);
        }

        private void TryAddHistoryPoint(
            List<(double value, double logL)> history,
            int paramIndex,
            double value,
            double[] mleParams)
        {
            if (!IsFinite(value))
                return;

            if (HistoryContainsValue(history, value))
                return;

            double logL = EvaluateLogLikelihood(value, paramIndex, mleParams);
            if (!IsFinite(logL))
                return;

            history.Add((value, logL));
        }

        private bool HistoryContainsValue(List<(double value, double logL)> history, double value)
        {
            double tol = _tolerance * Math.Max(1.0, Math.Abs(value));
            foreach (var point in history)
            {
                if (Math.Abs(point.value - value) <= tol)
                    return true;
            }

            return false;
        }

        private static double ChiSquaredDelta(double confidenceLevel)
        {
            // Use MathNet.Numerics for more accurate chi-squared inverse CDF
            var chi2 = new ChiSquared(1); // 1 degree of freedom
            return 0.5 * chi2.InverseCumulativeDistribution(confidenceLevel);
        }

        /// <summary>
        /// Plots the profile likelihood for a specific parameter using Plotly.NET (interactive HTML).
        /// </summary>
        /// <param name="paramIndex">Index of the parameter to plot</param>
        /// <param name="outputPath">Path to save the plot HTML (optional)</param>
        /// <param name="width">Plot width in pixels</param>
        /// <param name="height">Plot height in pixels</param>
        /// <param name="scanStep">Optional uniform scan step size for debug plotting</param>
        public void PlotProfileLikelihood(int paramIndex, string outputPath = null, int width = 800, int height = 600, double? scanStep = null)
        {
            if (!EnableDiagnostics || !ProfileHistories.ContainsKey(paramIndex) || ProfileHistories[paramIndex].Count == 0)
            {
                throw new InvalidOperationException("No profile history available. Set EnableDiagnostics to true before running.");
            }

            var baseHistory = ProfileHistories[paramIndex]
                .Where(p => IsFinite(p.value) && IsFinite(p.logL))
                .OrderBy(p => p.value)
                .ToList();
            Debug.WriteLine($"PlotProfileLikelihood paramIndex={paramIndex} points={baseHistory.Count}");
            for (int i = 0; i < baseHistory.Count; i++)
            {
                string valueText = baseHistory[i].value.ToString("G17", CultureInfo.InvariantCulture);
                string logLText = baseHistory[i].logL.ToString("G17", CultureInfo.InvariantCulture);
                Debug.WriteLine($"[{i}] value={valueText}, logL={logLText}");
            }
            if (baseHistory.Count == 0)
            {
                throw new InvalidOperationException("No finite profile history available for plotting.");
            }

            double mleValue;
            double mleLogL;
            if (!_lastMleValues.TryGetValue(paramIndex, out mleValue) ||
                !_lastMleLogLs.TryGetValue(paramIndex, out mleLogL) ||
                !IsFinite(mleValue) || !IsFinite(mleLogL))
            {
                var mlePoint = baseHistory.OrderByDescending(p => p.logL).First();
                mleValue = mlePoint.value;
                mleLogL = mlePoint.logL;
            }

            double threshold = _lastTargetLogLs.TryGetValue(paramIndex, out var storedTarget) && IsFinite(storedTarget)
                ? storedTarget
                : mleLogL - _deltaLogL;

            var lowerBound = _mle.GetLowerBounds()[paramIndex];
            var upperBound = _mle.GetUpperBounds()[paramIndex];

            var plotHistory = baseHistory;
            List<(double value, double logL)> scanHistory = null;
            if (scanStep.HasValue)
            {
                double step = scanStep.Value;
                if (!IsFinite(step) || step <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(scanStep), "Scan step must be a positive, finite number.");
                }

                if (_lastMleParameters == null || _lastMleParameters.Length <= paramIndex)
                {
                    throw new InvalidOperationException("No stored MLE parameters available for scanning. Call GetConfidenceInterval before plotting with scanStep.");
                }

                double scanLower = lowerBound;
                double scanUpper = upperBound;
                if (!IsFinite(scanLower) || !IsFinite(scanUpper) || scanUpper <= scanLower)
                {
                    scanLower = baseHistory.Min(p => p.value);
                    scanUpper = baseHistory.Max(p => p.value);
                }

                if (!IsFinite(scanLower) || !IsFinite(scanUpper) || scanUpper <= scanLower)
                {
                    throw new InvalidOperationException("Unable to determine finite scan bounds for profile plotting.");
                }

                int estimatedPoints = (int)Math.Floor((scanUpper - scanLower) / step) + 1;
                const int maxScanPoints = 20000;
                if (estimatedPoints > maxScanPoints)
                {
                    throw new InvalidOperationException(
                        $"Scan step {step:G4} over range [{scanLower:G4}, {scanUpper:G4}] yields {estimatedPoints} points. Increase scanStep or reduce the range.");
                }

                scanHistory = new List<(double value, double logL)>(estimatedPoints + 1);
                double lastValue = double.NaN;
                void AddScanPoint(double value)
                {
                    double clamped = MathUtils.Clamp(value, scanLower, scanUpper);
                    if (IsFinite(lastValue))
                    {
                        double tol = _tolerance * Math.Max(1.0, Math.Abs(clamped));
                        if (Math.Abs(clamped - lastValue) <= tol)
                        {
                            return;
                        }
                    }
                    double logL = EvaluateLogLikelihood(clamped, paramIndex, _lastMleParameters);
                    if (IsFinite(logL))
                    {
                        scanHistory.Add((clamped, logL));
                        lastValue = clamped;
                    }
                }

                for (double v = scanLower; v <= scanUpper + step * 0.5; v += step)
                {
                    AddScanPoint(v);
                }

                if (scanHistory.Count == 0 || Math.Abs(scanHistory[scanHistory.Count - 1].value - scanUpper) > _tolerance)
                {
                    AddScanPoint(scanUpper);
                }

                plotHistory = scanHistory
                    .Where(p => IsFinite(p.value) && IsFinite(p.logL))
                    .OrderBy(p => p.value)
                    .ToList();

                if (plotHistory.Count == 0)
                {
                    throw new InvalidOperationException("No finite profile values available after scanning.");
                }
            }
            if (scanHistory != null)
            {
                Debug.WriteLine($"PlotProfileLikelihood paramIndex={paramIndex} scanPoints={plotHistory.Count}");
                for (int i = 0; i < plotHistory.Count; i++)
                {
                    string valueText = plotHistory[i].value.ToString("G17", CultureInfo.InvariantCulture);
                    string logLText = plotHistory[i].logL.ToString("G17", CultureInfo.InvariantCulture);
                    Debug.WriteLine($"[scan {i}] value={valueText}, logL={logLText}");
                }
            }

            // Extract data points
            double[] x = plotHistory.Select(p => p.value).ToArray();
            double[] y = plotHistory.Select(p => p.logL).ToArray();

            // Build Plotly.NET traces
            var profileLine = Line.init(Color: Color.fromString("#1f77b4"), Width: 2.0);
            var profileMarker = Marker.init(Size: 6);
            var profileChart = Chart2D.Chart.Line<double, double, string>(
                x: x,
                y: y,
                Name: "Profile Likelihood",
                Line: profileLine,
                ShowMarkers: true,
                Marker: profileMarker);

            var mleMarker = Marker.init(Color: Color.fromString("red"), Size: 10);
            var mleChart = Chart2D.Chart.Point<double, double, string>(
                x: new[] { mleValue },
                y: new[] { mleLogL },
                Name: "MLE",
                Marker: mleMarker);

            double xMin = plotHistory.First().value;
            double xMax = plotHistory.Last().value;
            if (IsFinite(lowerBound))
            {
                xMin = lowerBound;
            }
            if (IsFinite(upperBound))
            {
                xMax = upperBound;
            }
            if (xMax < xMin)
            {
                (xMin, xMax) = (xMax, xMin);
            }

            var thresholdLine = Line.init(
                Color: Color.fromString("green"),
                Width: 2.0,
                Dash: StyleParam.DrawingStyle.Dash);
            var thresholdChart = Chart2D.Chart.Line<double, double, string>(
                x: new[] { xMin, xMax },
                y: new[] { threshold, threshold },
                Name: "CI Threshold",
                Line: thresholdLine);

            var charts = new List<GenericChart>
            {
                profileChart,
                mleChart,
                thresholdChart
            };

            // Find and mark the confidence limits if available
            var sortedHistory = plotHistory.ToArray();
            var crossings = new List<double>();
            for (int i = 0; i < sortedHistory.Length - 1; i++)
            {
                double x1 = sortedHistory[i].value;
                double y1 = sortedHistory[i].logL - threshold;
                double x2 = sortedHistory[i + 1].value;
                double y2 = sortedHistory[i + 1].logL - threshold;

                if (!IsFinite(y1) || !IsFinite(y2))
                    continue;

                if (Math.Abs(y1) < _tolerance)
                    crossings.Add(x1);
                if (Math.Abs(y2) < _tolerance)
                    crossings.Add(x2);

                if (y1 * y2 < 0)
                {
                    double cross = Interpolate(sortedHistory[i].value, sortedHistory[i].logL,
                                               sortedHistory[i + 1].value, sortedHistory[i + 1].logL,
                                               threshold);
                    if (IsFinite(cross))
                    {
                        crossings.Add(cross);
                    }
                }
            }

            double mleX = mleValue;
            double xTol = _tolerance * Math.Max(1.0, Math.Abs(mleX));
            double? ciLower = null;
            double? ciUpper = null;

            var left = crossings.Where(value => value <= mleX + xTol).ToList();
            var right = crossings.Where(value => value >= mleX - xTol).ToList();

            if (left.Count > 0)
            {
                ciLower = left.Max();
            }

            if (right.Count > 0)
            {
                ciUpper = right.Min();
            }

            if (ciLower.HasValue)
            {
                var lowerCiMarker = Marker.init(Color: Color.fromString("orange"), Size: 8);
                var lowerChart = Chart2D.Chart.Point<double, double, string>(
                    x: new[] { ciLower.Value },
                    y: new[] { threshold },
                    Name: "Lower CI",
                    Marker: lowerCiMarker);
                charts.Add(lowerChart);
            }

            if (ciUpper.HasValue)
            {
                var upperCiMarker = Marker.init(Color: Color.fromString("purple"), Size: 8);
                var upperChart = Chart2D.Chart.Point<double, double, string>(
                    x: new[] { ciUpper.Value },
                    y: new[] { threshold },
                    Name: "Upper CI",
                    Marker: upperCiMarker);
                charts.Add(upperChart);
            }

            string paramName = ParameterNames.TryGetValue(paramIndex, out var name) ? name : $"Parameter {paramIndex}";

            var combined = Chart.Combine(charts);
            combined = Chart.WithTitle($"Profile Likelihood for {paramName}", null).Invoke(combined);
            if (IsFinite(lowerBound) && IsFinite(upperBound) && lowerBound < upperBound)
            {
                combined = Chart.WithXAxisStyle<double, double, double>(
                    TitleText: paramName,
                    MinMax: Tuple.Create(lowerBound, upperBound)).Invoke(combined);
            }
            else
            {
                combined = Chart.WithXAxisStyle<double, double, double>(
                    TitleText: paramName).Invoke(combined);
            }
            combined = Chart.WithYAxisStyle<double, double, double>(
                TitleText: "Log-Likelihood").Invoke(combined);
            combined = Chart.WithSize((double)width, (double)height).Invoke(combined);

            string targetPath = outputPath;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                string safeName = string.Join("_", paramName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                if (string.IsNullOrWhiteSpace(safeName))
                {
                    safeName = $"param_{paramIndex}";
                }

                targetPath = Path.Combine(
                    Path.GetTempPath(),
                    $"ProfileLikelihood_{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.html");
            }
            else
            {
                string extension = Path.GetExtension(targetPath);
                if (string.IsNullOrEmpty(extension) ||
                    (!extension.Equals(".html", StringComparison.OrdinalIgnoreCase) &&
                     !extension.Equals(".htm", StringComparison.OrdinalIgnoreCase)))
                {
                    targetPath += ".html";
                }
            }

            combined.SaveHtml(targetPath);
            Console.WriteLine($"Profile likelihood plot saved to: {targetPath}");
        }

        /// <summary>
        /// Linear interpolation between two points
        /// </summary>
        private double Interpolate(double x1, double y1, double x2, double y2, double y)
        {
            if (Math.Abs(y1 - y2) < 1e-10) return (x1 + x2) / 2;
            return x1 + (y - y1) * (x2 - x1) / (y2 - y1);
        }

        /// <summary>
        /// Gets a summary of the profile likelihood analysis for a parameter
        /// </summary>
        public string GetProfileSummary(int paramIndex)
        {
            if (!EnableDiagnostics || !ProfileHistories.ContainsKey(paramIndex) || ProfileHistories[paramIndex].Count == 0)
            {
                return "No profile data available. Set EnableDiagnostics to true before running.";
            }

            var history = ProfileHistories[paramIndex]
                .Where(p => IsFinite(p.value) && IsFinite(p.logL))
                .ToList();
            if (history.Count == 0)
            {
                return "No finite profile data available. Check log-likelihood evaluations for invalid values.";
            }
            double mleValue;
            double mleLogL;
            if (!_lastMleValues.TryGetValue(paramIndex, out mleValue) ||
                !_lastMleLogLs.TryGetValue(paramIndex, out mleLogL) ||
                !IsFinite(mleValue) || !IsFinite(mleLogL))
            {
                var mlePoint = history.OrderByDescending(p => p.logL).First();
                mleValue = mlePoint.value;
                mleLogL = mlePoint.logL;
            }

            double threshold = _lastTargetLogLs.TryGetValue(paramIndex, out var storedTarget) && IsFinite(storedTarget)
                ? storedTarget
                : mleLogL - _deltaLogL;

            string paramName = ParameterNames.TryGetValue(paramIndex, out var name) ? name : $"Parameter {paramIndex}";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Profile Likelihood Summary for {paramName}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"MLE Value: {mleValue:G4}");
            sb.AppendLine($"Max Log-Likelihood: {mleLogL:G4}");
            sb.AppendLine($"CI Threshold (ΔLogL = {_deltaLogL:G4}): {threshold:G4}");

            // Find confidence limits
            var sortedHistory = history.OrderBy(p => p.value).ToArray();
            double? ciLower = null;
            double? ciUpper = null;

            var crossings = new List<double>();
            for (int i = 0; i < sortedHistory.Length - 1; i++)
            {
                double x1 = sortedHistory[i].value;
                double y1 = sortedHistory[i].logL - threshold;
                double x2 = sortedHistory[i + 1].value;
                double y2 = sortedHistory[i + 1].logL - threshold;

                if (!IsFinite(y1) || !IsFinite(y2))
                    continue;

                if (Math.Abs(y1) < _tolerance)
                    crossings.Add(x1);
                if (Math.Abs(y2) < _tolerance)
                    crossings.Add(x2);

                if (y1 * y2 < 0)
                {
                    double cross = Interpolate(sortedHistory[i].value, sortedHistory[i].logL,
                                               sortedHistory[i + 1].value, sortedHistory[i + 1].logL,
                                               threshold);
                    if (IsFinite(cross))
                    {
                        crossings.Add(cross);
                    }
                }
            }

            double mleX = mleValue;
            double xTol = _tolerance * Math.Max(1.0, Math.Abs(mleX));

            var left = crossings.Where(x => x <= mleX + xTol).ToList();
            var right = crossings.Where(x => x >= mleX - xTol).ToList();

            if (left.Count > 0)
            {
                ciLower = left.Max();
            }

            if (right.Count > 0)
            {
                ciUpper = right.Min();
            }

            if (ciLower.HasValue)
            {
                sb.AppendLine($"Lower CI Limit: {ciLower.Value:G4}");
            }
            else
            {
                sb.AppendLine("Lower CI Limit: At parameter bound");
            }

            if (ciUpper.HasValue)
            {
                sb.AppendLine($"Upper CI Limit: {ciUpper.Value:G4}");
            }
            else
            {
                sb.AppendLine("Upper CI Limit: At parameter bound");
            }

            sb.AppendLine($"Number of Profile Points: {history.Count}");

            return sb.ToString();
        }
    }
}
