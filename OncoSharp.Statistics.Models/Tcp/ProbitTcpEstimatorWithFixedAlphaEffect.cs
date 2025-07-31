// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Statistics.Models.Tcp.Parameters;

namespace OncoSharp.Statistics.Models.Tcp
{
    public class ProbitTcpEstimatorWithFixedAlphaEffect : ProbitTcpEstimator
    {
        public double AlphaVolumeEffect { get; }

        public ProbitTcpEstimatorWithFixedAlphaEffect(DoseValue alphaOverBeta, int numberOfMultipleStarts, double alphaVolumeEffect) : base(alphaOverBeta, numberOfMultipleStarts)
        {
            AlphaVolumeEffect = alphaVolumeEffect;
        }

        protected override double[] GetInitialParameters()
        {
            return new double[] { 0.0, 0.0, -10.0 };
        }

        protected override double[] GetLowerBounds()
        {
            return new double[] { 0.0, 0.0, -10.0 };
        }

        protected override double[] GetUpperBounds()
        {
            return new double[] { 200, 30, -10.0 };
        }

        protected override ProbitTcpParameters ConvertVectorToParameters(double[] parameters)
        {
            parameters[2] = AlphaVolumeEffect;
            return base.ConvertVectorToParameters(parameters);
        }
    }
}