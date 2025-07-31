// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Statistics.Models.Tcp.Parameters;

namespace OncoSharp.Statistics.Models.Tcp
{
    public class PoissonTcpEstimatorWithFixedAlpha : PoissonTcpEstimator
    {
        public double Alpha { get; }

        public PoissonTcpEstimatorWithFixedAlpha(DoseValue alphaOverBeta, int numberOfMultipleStarts, double alpha) : base(alphaOverBeta, numberOfMultipleStarts)
        {
            Alpha = alpha;
        }

        protected override double[] GetInitialParameters()
        {
            return new double[] { 0.12, 1 }; // Alpha, Log10ClonogenDensity
        }

        protected override double[] GetLowerBounds()
        {
            return new double[] { 0.12, 0 }; // reasonable biological bounds
        }

        protected override double[] GetUpperBounds()
        {
            return new double[] { 0.12, 10 };
        }

        protected override PoissonTcpParameters ConvertVectorToParameters(double[] x)
        {
            x[0] = Alpha;
            return base.ConvertVectorToParameters(x);
        }
    }
}