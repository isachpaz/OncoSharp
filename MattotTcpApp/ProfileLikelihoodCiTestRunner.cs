using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Abstractions.Models;
using OncoSharp.Statistics.Abstractions.ConfidenceInterval;
using OncoSharp.Statistics.Abstractions.MLEEstimators;

internal static class ProfileLikelihoodCiTestRunner
{
    public static void Run()
    {
        Console.WriteLine("ProfileLikelihoodCI bound proximity tests");
        Console.WriteLine("=========================================");

        RunBernoulliCase(
            label: "Normal case",
            successes: 970,
            trials: 1000,
            lowerBound: 0.95,
            upperBound: 0.99,
            scanStep: 0.0005);

        RunBernoulliCase(
            label: "Upper bound tight",
            successes: 98,
            trials: 100,
            lowerBound: 0.95,
            upperBound: 0.99,
            scanStep: 0.0005);

        RunBernoulliCase(
            label: "Lower bound tight",
            successes: 2,
            trials: 100,
            lowerBound: 0.01,
            upperBound: 0.05,
            scanStep: 0.0005);

        RunBernoulliCase(
            label: "Both bounds tight",
            successes: 50,
            trials: 100,
            lowerBound: 0.45,
            upperBound: 0.55,
            scanStep: 0.0005);
    }

    private static void RunBernoulliCase(
        string label,
        int successes,
        int trials,
        double lowerBound,
        double upperBound,
        double? scanStep)
    {
        if (trials <= 0) throw new ArgumentOutOfRangeException(nameof(trials));
        if (successes < 0 || successes > trials) throw new ArgumentOutOfRangeException(nameof(successes));

        Console.WriteLine();
        Console.WriteLine($"Case: {label}");
        Console.WriteLine($"Trials={trials}, Successes={successes}, Bounds=[{lowerBound:G6}, {upperBound:G6}]");

        var observations = BuildObservations(successes, trials);
        var inputData = Enumerable.Repeat(0.0, trials).ToList();

        var estimator = new BernoulliEstimator(lowerBound, upperBound);
        var profileCI = estimator.GetProfileLikelihoodCI(inputData, observations, confidenceLevel: 0.95, maxIterations: 200);
        profileCI.EnableDiagnostics = true;
        profileCI.SetParameterName(0, "p");
        profileCI.SetParameterStepSize(0, Math.Max(1e-4, (upperBound - lowerBound) / 50.0));

        double mleValue = successes / (double)trials;
        var mleParams = new[] { mleValue };

        var (lower, upper) = profileCI.GetConfidenceInterval(0, mleValue, mleParams);

        Console.WriteLine($"MLE p={mleValue:G6}");
        Console.WriteLine($"CI lower={lower:G6}, upper={upper:G6}");
        Console.WriteLine($"Distance to bounds: lower={lower - lowerBound:G6}, upper={upperBound - upper:G6}");

        Console.WriteLine("Profile summary:");
        Console.WriteLine(profileCI.GetProfileSummary(0));

        if (scanStep.HasValue)
        {
            profileCI.PlotProfileLikelihood(0, scanStep: scanStep.Value);
        }
    }

    private static List<bool> BuildObservations(int successes, int trials)
    {
        var observations = new List<bool>(trials);
        for (int i = 0; i < successes; i++)
        {
            observations.Add(true);
        }
        for (int i = successes; i < trials; i++)
        {
            observations.Add(false);
        }

        return observations;
    }

    private sealed class BernoulliEstimator : MaximumLikelihoodEstimator<double, BernoulliParameters>
    {
        private readonly double _lowerBound;
        private readonly double _upperBound;

        public BernoulliEstimator(double lowerBound, double upperBound)
        {
            if (upperBound <= lowerBound)
            {
                throw new ArgumentOutOfRangeException(nameof(upperBound), "Upper bound must exceed lower bound.");
            }

            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        protected override IOptimizer CreateSolver(int parameterCount)
        {
            return new NoOpOptimizer();
        }

        protected override (bool isNeeded, double penalityValue) Penalize(BernoulliParameters parameters)
        {
            return (false, 0.0);
        }

        protected override double[] GetInitialParameters()
        {
            return new[] { (_lowerBound + _upperBound) * 0.5 };
        }

        protected override double[] GetLowerBounds()
        {
            return new[] { _lowerBound };
        }

        protected override double[] GetUpperBounds()
        {
            return new[] { _upperBound };
        }

        protected override BernoulliParameters ConvertVectorToParameters(double[] parameters)
        {
            return new BernoulliParameters { P = parameters[0] };
        }

        protected override double LogLikelihood(
            BernoulliParameters parameters,
            IList<bool> observations,
            IList<double> inputData)
        {
            double p = parameters.P;
            if (p <= 0.0 || p >= 1.0)
            {
                return double.NegativeInfinity;
            }

            int successes = observations.Count(o => o);
            int failures = observations.Count - successes;

            return successes * Math.Log(p) + failures * Math.Log(1.0 - p);
        }

        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? logLikelihood)
        {
            return new[] { double.NaN };
        }
    }

    private sealed class BernoulliParameters
    {
        public double P { get; set; }
    }

    private sealed class NoOpOptimizer : IOptimizer
    {
        public IOptimizer SetMaxObjective(Func<double[], double> objective) => this;
        public IOptimizer SetLowerBounds(double[] lowerBounds) => this;
        public IOptimizer SetUpperBounds(double[] upperBounds) => this;

        public OptimizationResult Maximize(double[] initialGuess)
        {
            throw new NotSupportedException("Optimization is not required for single-parameter profile CI tests.");
        }

        public OptimizationResult MaximizeFromSingleStart(double[] initialGuess)
        {
            throw new NotSupportedException("Optimization is not required for single-parameter profile CI tests.");
        }
    }
}
