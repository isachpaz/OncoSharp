// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Models.Tcp.Parameters
{
    public class NiemierkoTcpParameters : IParameterMapper<NiemierkoTcpParameters>
    {

        public double D50 { get; internal set; }
        public double Gamma50 { get; internal set; }
        public double AlphaVolumeEffect { get; internal set; }

        public NiemierkoTcpParameters()
        {
        }

        public NiemierkoTcpParameters(double d50, double gamma50, double alphaVolumeEffect)
        {
            D50 = d50;
            Gamma50 = gamma50;
            AlphaVolumeEffect = alphaVolumeEffect;
        }

        public NiemierkoTcpParameters FromArray(double[] parameters)
        {
            return new NiemierkoTcpParameters(parameters[0], parameters[1], parameters[2]);
        }

        public double[] ToArray(NiemierkoTcpParameters parameters)
        {
            return new double[] { parameters.D50, parameters.Gamma50, parameters.AlphaVolumeEffect };
        }

        public int GetParametersCount()
        {
            return 3;
        }

        public string[] ParameterNames => new string[] { "D50", "Gamma50", "AlphaVolumeEffect" };
    }
}