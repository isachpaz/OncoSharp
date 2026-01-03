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

namespace OncoSharp.Statistics.Models.Tcp
{
    public class ProbitTcpEstimator : TcpMaximumLikelihoodEstimator<IPlanItem, ProbitTcpParameters>
    {
        public DoseValue AlphaOverBeta { get; }
        public int NumberOfMultipleStarts { get; }

        /// <summary>
        /// Function to extract the structure ID from the given plan item.
        /// Default is a constant "GTV".
        /// </summary>
        public Func<IPlanItem, string> StructureSelector { get; set; } = _ => "GTV";


        public ProbitTcpEstimator(DoseValue alphaOverBeta, int numberOfMultipleStarts)
        {
            AlphaOverBeta = alphaOverBeta;
            NumberOfMultipleStarts = numberOfMultipleStarts;
            if (base._parameterMapper == null)
            {
                base._parameterMapper = new ProbitTcpParameters();
            }
        }

        protected override IOptimizer CreateSolver(int parameterCount)
        {
            // return new SimplexGlobalOptimizer(numberOfMultipleStarts: NumberOfMultipleStarts);
            //return new NLoptOptimizer(NLoptAlgorithm.GN_ISRES, parameterCount, 1e-12, 10_000);
            return new NloptMultiStartLocalOptimizer();
        }

        private const double BadLL = -1e100;

        protected override (bool isNeeded, double penalityValue) Penalize(ProbitTcpParameters parameters)
        {
            if (parameters.D50 <= 0.0 ||
                parameters.Gamma50 < 0.0)
                return (true, BadLL);

            return (false, Double.NaN);
        }

        protected override double[] GetLowerBounds()
        {
            return new double[] { 1e-3, 0.0, -10.0 };
        }

        protected override double[] GetUpperBounds()
        {
            return new double[] { 200.0, 10, -10.0 };
        }

        protected override double[] GetInitialParameters()
        {
            
            double d50Init = 50.0;
            double gammaInit = 2.0;
            return new double[] { d50Init, gammaInit, -10.0 };
            
        }

        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? logLikelihood)
        {
            return new double[] { Double.NaN, Double.NaN, Double.NaN };
        }

        public override double ComputeTcp(ProbitTcpParameters parameters, IPlanItem planItem)
        {
            //if (parameters.D50 < 1e-3)
            //{
            //    return 0;
            //}

            if (parameters.D50 <= 0.0) return 0.5; // or return 0.0, but better is penalize in LL
            if (parameters.Gamma50 < 0.0) return 0.5;


            var structureId = StructureSelector(planItem);
            Geud2GyModel geudModel = new Geud2GyModel(parameters.AlphaVolumeEffect);
            var model = new TcpProbitModel( geudModel, parameters.D50, parameters.Gamma50);
            var points = planItem.CalculateEqd2DoseDistribution(structureId, AlphaOverBeta);
            var tcp = model.ComputeTcp(points);
            if (tcp.Value < 0.2)
            {
                Debug.WriteLine("");
            }
            return tcp.Value;
        }
    }
}