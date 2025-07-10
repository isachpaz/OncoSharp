// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Models.Tcp.Parameters
{
    public class D50GammaTcpParameters : IParameterMapper<D50GammaTcpParameters>
    {
        public double D50 { get; set; }
        public double Gamma { get; set; }

        public override string ToString()
        {
            return $"{nameof(D50)}: {D50}, {nameof(Gamma)}: {Gamma}";
        }

        public D50GammaTcpParameters FromArray(double[] parameters)
        {
            return new D50GammaTcpParameters { D50 = parameters[0], Gamma = parameters[1] };
        }

        public double[] ToArray(D50GammaTcpParameters parameters)
        {
            return new[] { parameters.D50, parameters.Gamma };
        }

        public string[] ParameterNames => new[] { "D50", "Gamma" };
    }
}