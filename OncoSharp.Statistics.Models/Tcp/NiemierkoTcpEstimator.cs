// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.Tcp.Parameters;
using System;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Radiobiology.GEUD;
using OncoSharp.Radiobiology.TCP;

namespace OncoSharp.Statistics.Models.Tcp
{
    public class NiemierkoTcpEstimator : TcpMaximumLikelihoodEstimator<IPlanItem, NiemierkoTcpParameters>
    {
        public DoseValue AlphaOverBeta { get; }
        public int NumberOfMultipleStarts { get; }

        /// <summary>
        /// Function to extract the structure ID from the given plan item.
        /// Default is a constant "GTV".
        /// </summary>
        public Func<IPlanItem, string> StructureSelector { get; set; } = _ => "GTV";

        public NiemierkoTcpEstimator(DoseValue alphaOverBeta, int numberOfMultipleStarts)
        {
            AlphaOverBeta = alphaOverBeta;
            NumberOfMultipleStarts = numberOfMultipleStarts;
            if (base._parameterMapper == null)
            {
                base._parameterMapper = new NiemierkoTcpParameters();
            }
        }

        protected override IOptimizer CreateSolver(int parameterCount)
        {
            return new SimplexGlobalOptimizer(numberOfMultipleStarts: NumberOfMultipleStarts);
        }

        protected override double[] GetInitialParameters()
        {
            return new double[] { 0.0, 0.0, -10.0 };
        }

        protected override double[] GetLowerBounds()
        {
            return new double[] { 0.0, 0.0, -200.0 };
        }

        protected override double[] GetUpperBounds()
        {
            return new double[] { 200, 30, 0.0 };
        }

        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? logLikelihood)
        {
            return new double[] { Double.NaN, Double.NaN, Double.NaN };
        }

        public override double ComputeTcp(NiemierkoTcpParameters parameters, IPlanItem planItem)
        {
            var structureId = StructureSelector(planItem);
            Geud2GyModel geudModel = new Geud2GyModel(parameters.AlphaVolumeEffect);
            TcpNiemierkoModel model = new TcpNiemierkoModel(geudModel, parameters.D50.Gy_Eqd2(), parameters.Gamma50);
            var tcp = model.ComputeTcp(planItem.CalculateEqd2DoseDistribution(structureId, AlphaOverBeta));
            return tcp.Value;
        }
    }
}