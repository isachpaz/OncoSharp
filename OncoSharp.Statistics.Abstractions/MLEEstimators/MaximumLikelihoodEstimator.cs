// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Statistics.Abstractions.ConfidenceInterval;
using OncoSharp.Statistics.Abstractions.Helpers;
using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Abstractions.MLEEstimators
{
    /// <summary>
    /// Abstract base class for performing maximum likelihood estimation (MLE) using a generic optimizer.
    /// </summary>
    /// <typeparam name="TData">The type of input data for the model.</typeparam>
    /// <typeparam name="TParameters">The type representing model parameters.</typeparam>
    public abstract class MaximumLikelihoodEstimator<TData, TParameters> :
        IMleInternals<TData, TParameters>
        where TParameters : new()
    {
        /// <summary>
        /// Fits the model to the provided data using MLE.
        /// </summary>
        /// <param name="observations">Observed binary outcomes.</param>
        /// <param name="inputData">Input data corresponding to each observation.</param>
        /// <param name="onImprovedSolution">Optional callback invoked when a better solution is found.</param>
        /// <returns>The result of the MLE fit, including parameters and log-likelihood.</returns>
        public MleResult<TParameters> Fit(
            IList<bool> observations,
            IList<TData> inputData,
            Action<MleResult<TParameters>> onImprovedSolution = null)
        {
            double[] initialParams = GetInitialParameters();
            double[] lowerBounds = GetLowerBounds();
            double[] upperBounds = GetUpperBounds();

            var optimizer = CreateSolver(initialParams.Length);

            double bestLogLikelihood = double.NegativeInfinity;
            TParameters bestParams = default;

            optimizer
                .SetLowerBounds(lowerBounds)
                .SetUpperBounds(upperBounds)
                .SetMaxObjective(parameters =>
                {
                    var paramObj = ConvertVectorToParameters(parameters);
                    var logLikelihood = LogLikelihood(paramObj, observations, inputData);

                    if (logLikelihood > bestLogLikelihood)
                    {
                        bestLogLikelihood = logLikelihood;
                        bestParams = paramObj;
                        onImprovedSolution?.Invoke(new MleResult<TParameters>(
                            parameters: bestParams,
                            logLikelihood: logLikelihood));
                    }

                    return logLikelihood;
                });

            var result = optimizer.Maximize(initialParams);


            return new MleResult<TParameters>(
                parameters: bestParams,
                standardErrors: CalculateStandardErrors(result.OptimizedParameters, result.ObjectiveValue),
                logLikelihood: result.ObjectiveValue,
                optResult: result);
        }

        /// <summary>
        /// Creates the optimizer instance. Override to customize optimization algorithm or settings.
        /// </summary>
        protected abstract IOptimizer CreateSolver(int parameterCount);

        /// <summary>
        /// Returns the initial parameter vector for optimization.
        /// </summary>
        protected abstract double[] GetInitialParameters();

        /// <summary>
        /// Returns the lower bounds for parameters, or null if unbounded.
        /// </summary>
        protected abstract double[] GetLowerBounds();

        /// <summary>
        /// Returns the upper bounds for parameters, or null if unbounded.
        /// </summary>
        protected abstract double[] GetUpperBounds();


        /// <summary>
        /// Converts a parameter vector to the parameter object.
        /// </summary>
        protected virtual TParameters ConvertVectorToParameters(double[] parameters)
        {
            return ParameterMapper.FromArray(parameters);
        }

        /// <summary>
        /// Computes the log-likelihood for the given parameters and data.
        /// </summary>
        protected abstract double LogLikelihood(
            TParameters parameters,
            IList<bool> observations,
            IList<TData> inputData);

        /// <summary>
        /// Calculates standard errors for the optimized parameters.
        /// </summary>
        protected abstract double[] CalculateStandardErrors(double[] optimizedParams, double? logLikelihood);

        protected IParameterMapper<TParameters> _parameterMapper = null;

        protected virtual IParameterMapper<TParameters> ParameterMapper =>
            _parameterMapper ?? (_parameterMapper = new ReflectionParameterMapper<TParameters>());


        // Explicit interface implementation — not visible on public API
        double[] IMleInternals<TData, TParameters>.GetLowerBounds() =>
            GetLowerBounds() ?? Enumerable.Repeat(double.NegativeInfinity, GetInitialParameters().Length).ToArray();

        double[] IMleInternals<TData, TParameters>.GetUpperBounds() =>
            GetUpperBounds() ?? Enumerable.Repeat(double.PositiveInfinity, GetInitialParameters().Length).ToArray();

        double IMleInternals<TData, TParameters>.LogLikelihood(TParameters parameters, IList<bool> observations,
            IList<TData> inputData) =>
            LogLikelihood(parameters, observations, inputData);

        TParameters IMleInternals<TData, TParameters>.ConvertVectorToParameters(double[] parameters) =>
            ConvertVectorToParameters(parameters);

        IOptimizer IMleInternals<TData, TParameters>.CreateSolver(int parameterCount) =>
            CreateSolver(parameterCount);

        public virtual ProfileLikelihoodCI<TData, TParameters> GetProfileLikelihoodCI(
            IList<TData> inputData,
            IList<bool> observations,
            double confidenceLevel = 0.95)
        {
            return new ProfileLikelihoodCI<TData, TParameters>(this, inputData, observations, confidenceLevel);
        }
    }
}