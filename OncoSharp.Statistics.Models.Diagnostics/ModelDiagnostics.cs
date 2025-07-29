// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using HosmerLemeshowTest;
using OncoSharp.Statistics.Abstractions.MLEEstimators;

namespace OncoSharp.Statistics.Models.Diagnostics
{
    public static class ModelDiagnostics
    {
        private static readonly HosmerLemeshowTest.HosmerLemeshowTest _hosmerLemeshowTest = new HosmerLemeshowTest.HosmerLemeshowTest();

        /// <summary>
        /// Performs the Hosmer-Lemeshow goodness-of-fit test for binary outcomes.
        /// </summary>
        /// <param name="predictedProbabilities">Predicted probabilities from the model.</param>
        /// <param name="actualOutcomes">Observed outcomes as 0/1 integer array.</param>
        /// <returns>HosmerLemeshowResult containing test statistics.</returns>
        public static HosmerLemeshowResult CalculateHosmerLemeshow(double[] predictedProbabilities, int[] actualOutcomes)
        {
            if (predictedProbabilities == null) throw new ArgumentNullException(nameof(predictedProbabilities));
            if (actualOutcomes == null) throw new ArgumentNullException(nameof(actualOutcomes));
            if (predictedProbabilities.Length != actualOutcomes.Length)
                throw new ArgumentException("Predicted and actual arrays must be the same length.");

            return _hosmerLemeshowTest.CalculateHosmerLemeshow(predictedProbabilities, actualOutcomes);
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

            return CalculateHosmerLemeshow(predictedProbabilities, actualOutcomes);
        }
    }
}
