using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Models.Diagnostics
{
    public class JacobianDiagnostics
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
        public static JacobianDiagnostics EstimateJacobianRankAtPoint<TData, TParameters>(
            IMleInternals<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            double[] evaluationPoint,
            double[] perturbations = null,
            double threshold = 1e-10) where TParameters : new()
        {
            var xEval = evaluationPoint;
            int paramCount = xEval.Length;
            int dataCount = inputData.Count;

            if (perturbations == null)
            {
                perturbations = xEval.Select(v => Math.Max(1e-6, 1e-6 * Math.Abs(v))).ToArray();
            }

            var jacobian = DenseMatrix.Create(dataCount, paramCount, 0.0);
            var diff = new NumericalDerivative();

            for (int i = 0; i < dataCount; i++)
            {
                var dataPoint = inputData[i];
                var observed = observations[i];

                // Log-likelihood contribution from a single data point
                Func<double[], double> logLik_i = x =>
                {
                    var p = estimator.ConvertVectorToParameters(x);
                    return estimator.LogLikelihood(p, new[] { observed }, new[] { dataPoint });
                };

                for (int j = 0; j < paramCount; j++)
                {
                    // Use perturbations[j] if you want adaptive step size in NumericalDerivative (not passed here though)
                    double derivative = diff.EvaluatePartialDerivative(logLik_i, xEval, j, 1);
                    jacobian[i, j] = derivative;
                }
            }

            var svd = jacobian.Svd(computeVectors: true);
            var singularValues = svd.S;
            int rank = singularValues.Count(s => s > threshold);

            return new JacobianDiagnostics(jacobian, singularValues, rank);
        }

        public static JacobianDiagnostics EstimateJacobianRank<TData, TParameters>(
            IMleInternals<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            double[] perturbations = null,
            double threshold = 1e-10) where TParameters : new()
        {
            var initialParameters = estimator.GetInitialParameters();
            return EstimateJacobianRankAtPoint(estimator, inputData, observations, initialParameters, perturbations, threshold);
        }
    }
}