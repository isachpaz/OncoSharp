// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Diagnostics;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.LinearAlgebra.Double;

namespace OncoSharp.Statistics.Identifiability
{
    using System;
    using MathNet.Numerics.Differentiation;
    using MathNet.Numerics.LinearAlgebra.Double;
    
    public static class NumericalUtils
    {
        /// <summary>
        /// Estimates the numerical rank of the Jacobian matrix of a vector-valued function at a given point.
        /// </summary>
        /// <param name="vectorFunction">A function f: ℝⁿ → ℝᵐ.</param>
        /// <param name="x0">The point in ℝⁿ to evaluate the Jacobian at.</param>
        /// <param name="tolerance">The singular value threshold for rank estimation.</param>
        /// <returns>The estimated rank of the Jacobian matrix.</returns>
        public static JacobianRankResult EstimateJacobianRank(Func<double[], double[]> vectorFunction, double[] x0, double tolerance = 1e-8)
        {
            var nd = new NumericalDerivative
            {
                StepSize = 1e-5,
                StepType = StepType.Relative
            };

            double[] fx = vectorFunction(x0);
            int outputDim = fx.Length;
            int inputDim = x0.Length;

            var jacobian = DenseMatrix.Create(outputDim, inputDim, 0.0);

            // Compute each row of the Jacobian (∂fi/∂xj)
            for (int i = 0; i < outputDim; i++)
            {
                int index = i; // Capture for closure
                Func<double[], double> fi = x => vectorFunction(x)[index];

                for (int j = 0; j < inputDim; j++)
                {
                    double partial = nd.EvaluatePartialDerivative(fi, x0, j, 1);
                    jacobian[i, j] = partial;
                }
            }

            // Compute rank from singular values
            var svd = jacobian.Svd(computeVectors: false);
            var singularValues = svd.S.ToArray();

            int rank = 0;
            foreach (var s in singularValues)
                if (s > tolerance)
                    rank++;

            Debug.WriteLine("Jacobian matrix:\n" + jacobian.ToMatrixString());
            Debug.WriteLine("Singular values: " + string.Join(", ", singularValues));
            Debug.WriteLine("Estimated rank: " + rank);

            return new JacobianRankResult
            {
                Jacobian = jacobian,
                SingularValues = singularValues,
                Rank = rank
            };
        }
    }
}