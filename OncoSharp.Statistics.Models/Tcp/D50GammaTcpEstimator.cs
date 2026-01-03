// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.MultistaerLocatolOpt;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.Radiobiology.TCP;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.Tcp.Parameters;
using System;
using System.Linq;

namespace OncoSharp.Statistics.Models.Tcp
{
    public class D50GammaTcpEstimator : TcpMaximumLikelihoodEstimator<IPlanItem, D50GammaTcpParameters>
    {
        public DoseValue AlphaOverBeta { get; }
        public int NumberOfMultipleStarts { get; }

        /// <summary>
        /// Function to extract the structure ID from the given plan item.
        /// Default is a constant "GTV".
        /// </summary>
        public Func<IPlanItem, string> StructureSelector { get; set; } = _ => "GTV";


        public D50GammaTcpEstimator(DoseValue alphaOverBeta, int numberOfMultipleStarts)
        {
            AlphaOverBeta = alphaOverBeta;
            NumberOfMultipleStarts = numberOfMultipleStarts;
            if (base._parameterMapper == null)
            {
                base._parameterMapper = new D50GammaTcpParameters();
            }
        }

        protected override (bool isNeeded, double penalityValue) Penalize(D50GammaTcpParameters parameters)
        {
            throw new NotImplementedException();
        }

        protected override double[] GetInitialParameters()
        {
            return new[] { 37.77, 3.56678 };
        }


        protected override double[] GetLowerBounds()
        {
            return new[] { 0.0, 0.0 };
        }

        protected override double[] GetUpperBounds()
        {
            return new[] { 200.0, 60.0 };
        }


        public override double ComputeTcp(D50GammaTcpParameters parameters, IPlanItem planItem)
        {
            var structureId = StructureSelector(planItem);
            var points = planItem.CalculateEqd2DoseDistribution(structureId, AlphaOverBeta);
           var model = new TcpPoissonD50GammaModel(parameters.D50, parameters.Gamma);
            var tcp = model.ComputeTcp(points.VoxelDoses);
            return tcp.Value;
        }

        private string GetStructureId(IPlanItem planItem)
        {
            return "GTV";
        }


        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? negLogLik)
        {
            return optimizedParams.Select(x => double.NaN).ToArray();
        }

        protected override IOptimizer CreateSolver(int parameterCount)
        {
            // return new SimplexGlobalOptimizer(numberOfMultipleStarts: NumberOfMultipleStarts);
            return new NloptMultiStartLocalOptimizer();
        }

      
    }
}