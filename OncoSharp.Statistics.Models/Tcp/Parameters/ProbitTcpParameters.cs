// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Statistics.Abstractions.Interfaces;
using System;

namespace OncoSharp.Statistics.Models.Tcp.Parameters
{
    public class ProbitTcpParameters : IParameterMapper<ProbitTcpParameters>
    {
        public double D50 { get; internal set; }
        public double Gamma50 { get; internal set; }
        public double AlphaVolumeEffect { get; internal set; }

        public ProbitTcpParameters()
        {
        }

        public ProbitTcpParameters(double d50, double gamma50, double alphaVolumeEffect)
        {
            D50 = d50;
            Gamma50 = gamma50;
            AlphaVolumeEffect = alphaVolumeEffect;
        }

        public ProbitTcpParameters FromArray(double[] parameters)
        {
            return new ProbitTcpParameters(parameters[0], parameters[1], parameters[2]);
        }

        //public ProbitTcpParameters FromArray(double[] parameters)
        //{
        //    return new ProbitTcpParameters(
        //        Math.Exp(parameters[0]),   // u -> D50
        //        Math.Exp(parameters[1]),   // v -> Gamma50
        //        parameters[2]);            // AlphaVolumeEffect unchanged (or fixed)
        //}


        public double[] ToArray(ProbitTcpParameters parameters)
        {
            return new double[] { parameters.D50, parameters.Gamma50, parameters.AlphaVolumeEffect };
        }

        private const double MinPos = 1e-300; // prevents -Infinity

        //public double[] ToArray(ProbitTcpParameters parameters)
        //{
        //    return new double[]
        //    {
        //        Math.Log(Math.Max(parameters.D50, MinPos)),
        //        Math.Log(Math.Max(parameters.Gamma50, MinPos)),
        //        parameters.AlphaVolumeEffect
        //    };
        //}



        public int GetParametersCount()
        {
            return 3;
        }

        public string[] ParameterNames => new string[] { "D50", "Gamma50", "AlphaVolumeEffect" };

        public override string ToString()
        {
            return
                $"{nameof(D50)}: {D50}, {nameof(Gamma50)}: {Gamma50}, {nameof(AlphaVolumeEffect)}: {AlphaVolumeEffect}";
        }
    }
}