// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Models.Tcp.Parameters
{
    public class D50GammaAlphaOverBetaTcpParameters : IParameterMapper<D50GammaAlphaOverBetaTcpParameters>
    {
        public double D50 { get; set; }
        public double Gamma { get; set; }
        public double AlphaOverBeta { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(D50)}: {D50}, {nameof(Gamma)}: {Gamma}, {nameof(AlphaOverBeta)}: {AlphaOverBeta}";
        }

        public D50GammaAlphaOverBetaTcpParameters FromArray(double[] parameters)
        {
            return new D50GammaAlphaOverBetaTcpParameters { D50 = parameters[0], Gamma = parameters[1], AlphaOverBeta = parameters[2]};
        }

        public double[] ToArray(D50GammaAlphaOverBetaTcpParameters parameters)
        {
            return new[] { parameters.D50, parameters.Gamma, parameters.AlphaOverBeta };
        }

        public string[] ParameterNames => new[] { "D50", "Gamma", "AlphaOverBeta" };
    }
}