// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;


namespace OncoSharp.Statistics.Models.General.Parameters
{
    public class LogisticParameters : IParameterMapper<LogisticParameters>
    {
        public double Beta0 { get; set; }
        public double Beta1 { get; set; }

        public override string ToString()
        {
            return $"{nameof(Beta0)}: {Beta0}, {nameof(Beta1)}: {Beta1}";
        }

        public LogisticParameters FromArray(double[] parameters)
        {
            return new LogisticParameters()
            {
                Beta0 = parameters[0],
                Beta1 = parameters[1],
            };
        }

        public double[] ToArray(LogisticParameters parameters)
        {
            return new double[] { parameters.Beta1, parameters.Beta1 };
        }

        public int GetParametersCount() => 2;

        public string[] ParameterNames => new string[] { "Beta0", "Beta1" };
    }
}