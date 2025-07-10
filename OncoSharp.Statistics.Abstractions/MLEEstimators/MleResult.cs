// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Optimization.Abstractions.Models;

namespace OncoSharp.Statistics.Abstractions.MLEEstimators
{
    public class MleResult<TParameters>
    {
        public TParameters Parameters { get; }
        public double[] StandardErrors { get; }
        public double? LogLikelihood { get; }
        public OptimizationResult OptResult { get; }

        public MleResult(TParameters parameters, double[] standardErrors,
            double? logLikelihood, OptimizationResult optResult)
        {
            Parameters = parameters;
            StandardErrors = standardErrors;
            LogLikelihood = logLikelihood;
            OptResult = optResult;
        }

        public MleResult(TParameters parameters, double logLikelihood)
            : this(parameters, null, logLikelihood, null)
        {
        }
    }
}