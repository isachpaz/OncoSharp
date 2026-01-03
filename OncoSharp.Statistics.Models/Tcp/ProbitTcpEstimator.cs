// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using NLoptNet;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.MultistaerLocatolOpt;
using OncoSharp.Optimization.Algorithms.NLopt;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.Radiobiology.GEUD;
using OncoSharp.Radiobiology.TCP;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.Tcp.Parameters;
using System;
using System.Diagnostics;
using OncoSharp.Statistics.Abstractions.Interfaces;
using MathNet.Numerics.Distributions;


namespace OncoSharp.Statistics.Models.Tcp
{
    public class ProbitTcpEstimator : TcpMaximumLikelihoodEstimator<IPlanItem, ProbitTcpParameters>
    {
        public DoseValue AlphaOverBeta { get; }
        public int NumberOfMultipleStarts { get; }

        public Func<IPlanItem, string> StructureSelector { get; set; } = _ => "GTV";

        private readonly bool _useLogSpace;

        private const double BadLL = -1e100;
        private const double MinGammaPos = 1e-6; // for log-space only

        public ProbitTcpEstimator(DoseValue alphaOverBeta, int numberOfMultipleStarts, bool useLogSpace)
        {
            AlphaOverBeta = alphaOverBeta;
            NumberOfMultipleStarts = numberOfMultipleStarts;
            _useLogSpace = useLogSpace;
            
            // Always set mapper explicitly (avoid stale base mapper)
            base._parameterMapper = useLogSpace
                ? (IParameterMapper<ProbitTcpParameters>)new LogSpaceProbitTcpMapper()
                : (IParameterMapper<ProbitTcpParameters>)new ProbitTcpParameters();
        }

        protected override IOptimizer CreateSolver(int parameterCount)
        {
            return new NloptMultiStartLocalOptimizer();
        }

        protected override (bool isNeeded, double penalityValue) Penalize(ProbitTcpParameters parameters)
        {
            // parameters here are ALWAYS physical (after ConvertVectorToParameters)
            if (parameters.D50 <= 0.0) return (true, BadLL);
            if (parameters.Gamma50 < 0.0) return (true, BadLL); // or <= 0 if you require strictly positive

            return (false, double.NaN);
        }

        public (double Lr, double PBoundary, double LogLikNull) BoundaryLrtGamma50EqualsZero(double logLikFull, int n)
        {
            // Null: Gamma50 = 0 => TCP = 0.5 constant in YOUR model
            double logLikNull = n * Math.Log(0.5);

            double lr = 2.0 * (logLikFull - logLikNull);
            if (lr < 0.0) lr = 0.0;

            // boundary mixture: 0.5*ChiSq0 + 0.5*ChiSq1
            double pWilks = 1.0 - ChiSquared.CDF(1, lr);
            double pBoundary = 0.5 * pWilks;

            return (lr, pBoundary, logLikNull);
        }


        protected override double[] GetLowerBounds()
        {
            if (_useLogSpace)
            {
                return new[]
                {
                    Math.Log(1e-3),             // log(D50)
                    Math.Log(MinGammaPos),      // log(Gamma50)  (must be > 0)
                    -10.0
                };
            }

            return new[]
            {
                1e-3,   // D50
                0.0,    // Gamma50 (allow 0 in physical mode)
                -10.0
            };
        }

        protected override double[] GetUpperBounds()
        {
            if (_useLogSpace)
            {
                return new[]
                {
                    Math.Log(200.0),  // log(D50)
                    Math.Log(10.0),   // log(Gamma50)
                    -10.0
                };
            }

            return new[]
            {
                200.0,  // D50
                10.0,   // Gamma50
                -10.0
            };
        }

        protected override double[] GetInitialParameters()
        {
            double d50Init = 50.0;
            double gammaInit = 2.0;

            if (_useLogSpace)
            {
                return new[]
                {
                    Math.Log(d50Init),
                    Math.Log(gammaInit),
                    -10.0
                };
            }

            return new[]
            {
                d50Init,
                gammaInit,
                -10.0
            };
        }

        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? logLikelihood)
        {
            return new[] { double.NaN, double.NaN, double.NaN };
        }

        public override double ComputeTcp(ProbitTcpParameters parameters, IPlanItem planItem)
        {
            // This should only ever see PHYSICAL parameters.
            // Penalize() already rejects invalid regions, but keep guards for safety:
            if (parameters.D50 <= 0.0) return 0.5;
            if (parameters.Gamma50 < 0.0) return 0.5;

            var structureId = StructureSelector(planItem);
            var geudModel = new Geud2GyModel(parameters.AlphaVolumeEffect);
            var model = new TcpProbitModel(geudModel, parameters.D50, parameters.Gamma50);

            var points = planItem.CalculateEqd2DoseDistribution(structureId, AlphaOverBeta);
            var tcp = model.ComputeTcp(points);

            return tcp.Value;
        }
    }
}
