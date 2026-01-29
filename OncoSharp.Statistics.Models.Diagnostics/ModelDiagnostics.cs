// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using HLTest.Core;
using OncoSharp.Statistics.Abstractions.MLEEstimators;

namespace OncoSharp.Statistics.Models.Diagnostics
{
    public static class ModelDiagnostics
    {
        private static readonly HosmerLemeshowTest _hosmerLemeshowTest = new HosmerLemeshowTest();

        /// <summary>
        /// Performs the Hosmer-Lemeshow goodness-of-fit test for binary outcomes.
        /// </summary>
        /// <param name="predictedProbabilities">Predicted probabilities from the model.</param>
        /// <param name="actualOutcomes">Observed outcomes as 0/1 integer array.</param>
        /// <returns>HosmerLemeshowResult containing test statistics.</returns>
        public static HosmerLemeshowResult CalculateHosmerLemeshow(double[] predictedProbabilities, int[] actualOutcomes, int numGroups = 10)
        {
            if (predictedProbabilities == null) throw new ArgumentNullException(nameof(predictedProbabilities));
            if (actualOutcomes == null) throw new ArgumentNullException(nameof(actualOutcomes));
            if (predictedProbabilities.Length != actualOutcomes.Length)
                throw new ArgumentException("Predicted and actual arrays must be the same length.");

            return _hosmerLemeshowTest.CalculateHosmerLemeshow(predictedProbabilities, actualOutcomes, numGroups);
        }

        /// <summary>
        /// Performs Hosmer-Lemeshow test for TcpMaximumLikelihoodEstimator using its ComputeTcp method.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TParameters"></typeparam>
        /// <param name="estimator">The TCP MLE estimator instance.</param>
        /// <param name="observations">Observed outcomes (true/false).</param>
        /// <param name="inputData">Input data for each observation.</param>
        /// <returns>HosmerLemeshowResult with the test statistics.</returns>
        public static HosmerLemeshowResult CalculateHosmerLemeshow<TData, TParameters>(
            TcpMaximumLikelihoodEstimator<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            TParameters bestParameters)
            where TParameters : new()
        {
            if (estimator == null) throw new ArgumentNullException(nameof(estimator));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));
            if (observations.Count != inputData.Count)
                throw new ArgumentException("Observations and inputData must have the same number of elements.");
            
            var predictedProbabilities = new double[inputData.Count];

            for (int i = 0; i < inputData.Count; i++)
            {
                double tcp = estimator.ComputeTcp(bestParameters, inputData[i]);
                predictedProbabilities[i] = tcp;
            }

            var actualOutcomes = observations.Select(b => b ? 1 : 0).ToArray();

            return CalculateHosmerLemeshow(predictedProbabilities, actualOutcomes,8);
        }

        /// <summary>
        /// Calculates ROC AUC (area under the ROC curve) from predicted probabilities and binary outcomes.
        /// </summary>
        /// <param name="predictedProbabilities">Predicted probabilities from the model.</param>
        /// <param name="actualOutcomes">Observed outcomes as 0/1 integer array.</param>
        /// <returns>AUC value in [0, 1].</returns>
        public static double CalculateAuc(double[] predictedProbabilities, int[] actualOutcomes)
        {
            ValidateInputs(predictedProbabilities, actualOutcomes);

            int positives = actualOutcomes.Count(v => v == 1);
            int negatives = actualOutcomes.Length - positives;
            if (positives == 0 || negatives == 0)
                throw new InvalidOperationException("AUC is undefined when all outcomes are the same class.");

            var pairs = predictedProbabilities
                .Select((score, idx) => new ScoreLabel(score, actualOutcomes[idx]))
                .OrderBy(p => p.Score)
                .ToArray();

            double rankSumPos = 0.0;
            int index = 0;
            while (index < pairs.Length)
            {
                int start = index;
                double score = pairs[index].Score;
                int posCount = 0;

                while (index < pairs.Length && AreScoresEqual(pairs[index].Score, score))
                {
                    if (pairs[index].Label == 1)
                        posCount++;
                    index++;
                }

                int end = index - 1;
                double avgRank = (start + end) / 2.0 + 1.0; // ranks are 1-based
                rankSumPos += posCount * avgRank;
            }

            double auc = (rankSumPos - positives * (positives + 1) / 2.0) / (positives * negatives);
            return auc;
        }

        /// <summary>
        /// Calculates ROC AUC using a TCP estimator and its ComputeTcp method.
        /// </summary>
        public static double CalculateAuc<TData, TParameters>(
            TcpMaximumLikelihoodEstimator<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            TParameters bestParameters)
            where TParameters : new()
        {
            if (estimator == null) throw new ArgumentNullException(nameof(estimator));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));
            if (observations.Count != inputData.Count)
                throw new ArgumentException("Observations and inputData must have the same number of elements.");

            var predictedProbabilities = new double[inputData.Count];
            for (int i = 0; i < inputData.Count; i++)
            {
                predictedProbabilities[i] = estimator.ComputeTcp(bestParameters, inputData[i]);
            }

            var actualOutcomes = observations.Select(b => b ? 1 : 0).ToArray();
            return CalculateAuc(predictedProbabilities, actualOutcomes);
        }

        /// <summary>
        /// Calculates the Brier score (mean squared error) for probabilistic predictions.
        /// </summary>
        /// <param name="predictedProbabilities">Predicted probabilities from the model.</param>
        /// <param name="actualOutcomes">Observed outcomes as 0/1 integer array.</param>
        /// <returns>Brier score (lower is better).</returns>
        public static double CalculateBrierScore(double[] predictedProbabilities, int[] actualOutcomes)
        {
            ValidateInputs(predictedProbabilities, actualOutcomes);

            double sum = 0.0;
            for (int i = 0; i < predictedProbabilities.Length; i++)
            {
                double p = predictedProbabilities[i];
                double y = actualOutcomes[i];
                double diff = p - y;
                sum += diff * diff;
            }

            return sum / predictedProbabilities.Length;
        }

        /// <summary>
        /// Calculates the Brier score using a TCP estimator and its ComputeTcp method.
        /// </summary>
        public static double CalculateBrierScore<TData, TParameters>(
            TcpMaximumLikelihoodEstimator<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            TParameters bestParameters)
            where TParameters : new()
        {
            if (estimator == null) throw new ArgumentNullException(nameof(estimator));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));
            if (observations.Count != inputData.Count)
                throw new ArgumentException("Observations and inputData must have the same number of elements.");

            var predictedProbabilities = new double[inputData.Count];
            for (int i = 0; i < inputData.Count; i++)
            {
                predictedProbabilities[i] = estimator.ComputeTcp(bestParameters, inputData[i]);
            }

            var actualOutcomes = observations.Select(b => b ? 1 : 0).ToArray();
            return CalculateBrierScore(predictedProbabilities, actualOutcomes);
        }

        private static void ValidateInputs(double[] predictedProbabilities, int[] actualOutcomes)
        {
            if (predictedProbabilities == null) throw new ArgumentNullException(nameof(predictedProbabilities));
            if (actualOutcomes == null) throw new ArgumentNullException(nameof(actualOutcomes));
            if (predictedProbabilities.Length != actualOutcomes.Length)
                throw new ArgumentException("Predicted and actual arrays must be the same length.");
            if (predictedProbabilities.Length == 0)
                throw new ArgumentException("Predicted and actual arrays must be non-empty.");

            for (int i = 0; i < predictedProbabilities.Length; i++)
            {
                double p = predictedProbabilities[i];
                if (double.IsNaN(p) || double.IsInfinity(p))
                    throw new ArgumentException("Predicted probabilities contain non-finite values.");
                if (p < 0.0 || p > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(predictedProbabilities), "Predicted probabilities must be within [0, 1].");
            }

            for (int i = 0; i < actualOutcomes.Length; i++)
            {
                int y = actualOutcomes[i];
                if (y != 0 && y != 1)
                    throw new ArgumentOutOfRangeException(nameof(actualOutcomes), "Actual outcomes must be 0 or 1.");
            }
        }

        private static bool AreScoresEqual(double a, double b)
        {
            double tol = 1e-12 * Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)));
            return Math.Abs(a - b) <= tol;
        }

        private readonly struct ScoreLabel
        {
            public ScoreLabel(double score, int label)
            {
                Score = score;
                Label = label;
            }

            public double Score { get; }
            public int Label { get; }
        }
    }
}
