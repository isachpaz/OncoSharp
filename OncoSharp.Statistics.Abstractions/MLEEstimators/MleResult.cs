// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Optimization.Abstractions.Models;

namespace OncoSharp.Statistics.Abstractions.MLEEstimators
{
    public class MleResult<TParameters>
    {
        public TParameters Parameters { get; }
        public double[] StandardErrors { get; }
        public double? LogLikelihood { get; }
        public OptimizationResult OptResult { get; }
        public double AIC { get; }
        public double BIC { get; }
        public double TotalObservations { get; }
        public double ObservationsEventTrue { get; }
        public double ObservationsEventFalse { get; }

        public MleResult(TParameters parameters, double[] standardErrors,
            double? logLikelihood, OptimizationResult optResult, double aic, double bic, int totalObservations,
            double observationsEventTrue,
            double observationsEventFalse)
        {
            Parameters = parameters;
            StandardErrors = standardErrors;
            LogLikelihood = logLikelihood;
            OptResult = optResult;
            AIC = aic;
            BIC = bic;
            TotalObservations = totalObservations;
            ObservationsEventTrue = observationsEventTrue;
            ObservationsEventFalse = observationsEventFalse;
        }

        public MleResult(TParameters parameters, double logLikelihood)
            : this(parameters, null, logLikelihood, null, Double.NaN, Double.NaN, -1, -1,-1)
        {
        }

    }
}