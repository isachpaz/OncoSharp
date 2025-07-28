// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;
using OncoSharp.Statistics.Abstractions.MLEEstimators;

namespace OncoSharp.Statistics.Identifiability
{
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    

    public class JacobianDiagnostics<TData, TParameters> where TParameters : new()
    {
        public Matrix<double> Jacobian { get; }
        public Vector<double> SingularValues { get; }
        public int Rank { get; }

        public JacobianDiagnostics(Matrix<double> jacobian, Vector<double> singularValues, int rank)
        {
            Jacobian = jacobian;
            SingularValues = singularValues;
            Rank = rank;
        }

        public override string ToString()
        {
            return $"Jacobian Matrix:\n{Jacobian.ToMatrixString()}\n" +
                   $"Singular Values: {string.Join(", ", SingularValues)}\n" +
                   $"Estimated Rank: {Rank}";
        }
    }

    public static class MleDiagnostics
    {
        public static JacobianDiagnostics<TData, TParameters> EstimateJacobianRank<TData, TParameters>(
            IMleInternals<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            double perturbation = 1e-5,
            double threshold = 1e-10) where TParameters : new()
        {
            // Get parameter vector and define the function to evaluate
            var x0 = estimator.GetInitialParameters();
            var n = x0.Length;

            // Evaluate the function at x0
            Func<double[], double> logLikFunc = x =>
            {
                var p = estimator.ConvertVectorToParameters(x);
                return estimator.LogLikelihood(p, observations, inputData);
            };

            // Finite difference Jacobian estimation
            var jacobian = DenseMatrix.Create(1, n, 0.0);
            var f0 = logLikFunc(x0);

            for (int i = 0; i < n; i++)
            {
                var xPerturbed = (double[])x0.Clone();
                xPerturbed[i] += perturbation;

                double fPerturbed = logLikFunc(xPerturbed);
                jacobian[0, i] = (fPerturbed - f0) / perturbation;
            }

            var svd = jacobian.Svd(true);
            var singularValues = svd.S;
            int rank = singularValues.AsEnumerable().Count(s => s > threshold);

            return new JacobianDiagnostics<TData, TParameters>(jacobian, singularValues, rank);
        }
    }
}