// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Linq;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.Radiobiology.TCP;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.Tcp.Parameters;

namespace OncoSharp.Statistics.Models.Tcp
{
    public class
        D50GammaAlphaOverBetaTcpEstimator : TcpMaximumLikelihoodEstimator<IPlanItem, D50GammaAlphaOverBetaTcpParameters>
    {
        /// <summary>
        /// Function to extract the structure ID from the given plan item.
        /// Default is a constant "GTV".
        /// </summary>
        public Func<IPlanItem, string> StructureSelector { get; set; } = _ => "GTV";


        public D50GammaAlphaOverBetaTcpEstimator()
        {
            if (base._parameterMapper == null)
            {
                base._parameterMapper = new D50GammaAlphaOverBetaTcpParameters();
            }
        }

        protected override double[] GetInitialParameters()
        {
            return new[] { 0.0, 0.0, 1.0 };
        }


        protected override double[] GetLowerBounds()
        {
            return new[] { 0.0, 0.0, 0.0 };
        }

        protected override double[] GetUpperBounds()
        {
            return new[] { 100.0, 60.0, 60.0 };
        }


        protected override double ComputeTcp(D50GammaAlphaOverBetaTcpParameters parameters, IPlanItem planItem)
        {
            var structureId = StructureSelector(planItem);
            var points = planItem.CalculateEqd2DoseDistribution(structureId, parameters.AlphaOverBeta.Gy());
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
            return new SimplexGlobalOptimizer();
        }
    }
}