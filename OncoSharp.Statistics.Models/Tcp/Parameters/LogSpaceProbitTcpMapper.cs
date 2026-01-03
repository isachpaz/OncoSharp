// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Models.Tcp.Parameters
{
    public sealed class LogSpaceProbitTcpMapper : IParameterMapper<ProbitTcpParameters>
    {
        private const double MinPos = 1e-300;

        public ProbitTcpParameters FromArray(double[] x)
        {
            // x is optimizer-space: [log(D50), log(Gamma50), Alpha]
            return new ProbitTcpParameters(
                d50: Math.Exp(x[0]),
                gamma50: Math.Exp(x[1]),
                alphaVolumeEffect: x[2]
            );
        }

        public double[] ToArray(ProbitTcpParameters p)
        {
            // convert physical -> optimizer-space
            return new double[]
            {
                Math.Log(Math.Max(p.D50, MinPos)),
                Math.Log(Math.Max(p.Gamma50, MinPos)),
                p.AlphaVolumeEffect
            };
        }

        public int GetParametersCount() => 3;

        public string[] ParameterNames =>
            new[] { "log(D50)", "log(Gamma50)", "AlphaVolumeEffect" };
    }
}