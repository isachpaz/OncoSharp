// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using OncoSharp.Statistics.Abstractions.Helpers;

namespace OncoSharp.Statistics.Abstractions.MLEEstimators
{
    public abstract class NtcpMaximumLikelihoodEstimator<TData, TParameters>
        : MaximumLikelihoodEstimator<TData, TParameters>
        where TParameters : new()
    {
        protected override double LogLikelihood(
            TParameters parameters,
            IList<bool> observations,
            IList<TData> inputData)
        {
            if (observations.Count != inputData.Count)
                throw new ArgumentException("Mismatch between observations and input data.");

            double logLik = 0.0;
            for (int i = 0; i < observations.Count; i++)
            {
                double ntcp = ComputeNtcp(parameters, inputData[i]);

                // Clamp NTCP to avoid log(0)
                ntcp = MathUtils.Clamp(ntcp, 1e-12, 1.0 - 1e-12);

                logLik += observations[i] ? Math.Log(ntcp) : Math.Log(1.0 - ntcp);
            }

            return logLik;
        }

        /// <summary>
        /// Computes the Normal Tissue Complication Probability (NTCP)
        /// for the given parameters and input data.
        /// Must be implemented by the subclass.
        /// </summary>
        protected abstract double ComputeNtcp(TParameters parameters, TData data);
    }
}