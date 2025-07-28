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
    public abstract class TcpMaximumLikelihoodEstimator<TData, TParameters>
        : MaximumLikelihoodEstimator<TData, TParameters> where TParameters : new()
    {
        /// <summary>
        /// 
        /// If the patient responded or disease free (observation = true), it adds log(tcp) — higher TCP → better fit.
        /// If the patient did not respond, or had a relapse (observation = false), it adds log(1 - tcp) — lower TCP → better fit.
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="observations"></param>
        /// <param name="inputData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
                double tcp = ComputeTcp(parameters, inputData[i]);

                tcp = MathUtils.Clamp(tcp, 1e-12, 1.0 - 1e-12);

                logLik += observations[i] ? Math.Log(tcp) : Math.Log(1.0 - tcp);
            }

            return logLik;
        }

        /// <summary>
        /// Computes the Tumor Control Probability (TCP) for the given parameters and input data.
        /// This must be implemented by derived classes.
        /// </summary>
        public abstract double ComputeTcp(TParameters parameters, TData data);

    }
}