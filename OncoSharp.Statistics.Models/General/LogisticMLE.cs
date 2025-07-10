// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.Statistics.Abstractions.Helpers;
using OncoSharp.Statistics.Abstractions.Interfaces;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.General.Parameters;
using System;
using System.Collections.Generic;

namespace OncoSharp.Statistics.Models.General
{
    public class LogisticMLE : MaximumLikelihoodEstimator<(double X1, double X2), LogisticParameters>
    {
        public LogisticMLE()
        {
            base._parameterMapper = new LogisticParameters();
        }

        protected override IOptimizer CreateSolver(int parameterCount)
        {
            return new SimplexGlobalOptimizer();
        }

        protected override double[] GetInitialParameters()
        {
            return new[] { 0.0, 0.0 }; // Start with zero coefficients
        }


        protected override double[] GetLowerBounds()
        {
            return new[] { -100.0, -100.0 };
        }

        protected override double[] GetUpperBounds()
        {
            return new[] { 100.0, 100.0 };
        }


        protected override double LogLikelihood(
            LogisticParameters parameters,
            IList<bool> observations,
            IList<(double X1, double X2)> inputData)
        {
            double logLik = 0.0;

            for (int i = 0; i < inputData.Count; i++)
            {
                double x0 = inputData[i].X1;
                double x1 = inputData[i].X2;
                bool y = observations[i];

                double linear = parameters.Beta0 * x0 + parameters.Beta1 * x1;
                double p = 1.0 / (1.0 + Math.Exp(-linear));
                p = MathUtils.Clamp(p, 1e-12, 1 - 1e-12); // Avoid log(0)

                logLik += y ? Math.Log(p) : Math.Log(1 - p);
            }

            return logLik;
        }

        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? negLogLik)
        {
            // Placeholder: Return zeros. Replace with Hessian-based standard error calculation if needed.
            return new double[optimizedParams.Length];
        }
    }
}