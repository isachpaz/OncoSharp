// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using MathNet.Numerics.LinearAlgebra;

namespace OncoSharp.Statistics.Identifiability
{
    public class JacobianRankResult
    {
        /// <summary>The full Jacobian matrix evaluated at x₀.</summary>
        public Matrix<double> Jacobian { get; set; }

        /// <summary>The singular values of the Jacobian matrix.</summary>
        public double[] SingularValues { get; set; }

        /// <summary>The estimated numerical rank based on the given tolerance.</summary>
        public int Rank { get; set; }

        public override string ToString()
        {
            return $"Jacobian matrix:\n{Jacobian.ToMatrixString()}\n" +
                   $"Singular values: {string.Join(", ", SingularValues)}\n" +
                   $"Estimated rank: {Rank}";
        }
    }
}