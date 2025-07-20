// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;
using System;

namespace OncoSharp.Statistics.Models.Ntcp.Parameters
{
    public class LkbNtcpParameters : IParameterMapper<LkbNtcpParameters>
    {
        public double TD50 { get; set; }
        public double M { get; set; }
        public double N { get; set; }

        public override string ToString()
        {
            return $"{nameof(TD50)}: {TD50}, {nameof(M)}: {M}, {nameof(N)}: {N}";
        }

        public LkbNtcpParameters FromArray(double[] parameters)
        {
            throw new NotImplementedException();
        }

        public double[] ToArray(LkbNtcpParameters parameters)
        {
            throw new NotImplementedException();
        }

        public int GetParametersCount() => 3;
        

        public string[] ParameterNames { get; }
    }
}