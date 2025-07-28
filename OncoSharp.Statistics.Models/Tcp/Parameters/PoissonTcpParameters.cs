// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Models.Tcp.Parameters
{
    public class PoissonTcpParameters : IParameterMapper<PoissonTcpParameters>
    {
        public double Alpha { get; set; }  // Linear LQ parameter [Gy⁻¹]
        public double LogClonogenDensity { get; set; }  // [clonogens/cm³]

        public override string ToString()
        {
            return $"{nameof(Alpha)}: {Alpha}, {nameof(LogClonogenDensity)}: {LogClonogenDensity:E2}";
        }

        public PoissonTcpParameters FromArray(double[] parameters)
        {
            return new PoissonTcpParameters() { Alpha = parameters[0], LogClonogenDensity = parameters[1] };
        }

        public double[] ToArray(PoissonTcpParameters parameters)
        {
            return new double[]{parameters.Alpha, parameters.LogClonogenDensity };
        }

        public int GetParametersCount()
        {
            return 2;
        }

        public string[] ParameterNames => new String[] { "Alpha", "LogClonogenDensity" };
    }
}