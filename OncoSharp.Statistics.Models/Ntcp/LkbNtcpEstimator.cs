// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Helpers.Maths;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.Ntcp.Parameters;
using System;
using System.IO;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Radiobiology.GEUD;

namespace OncoSharp.Statistics.Models.Ntcp
{
    public class LkbNtcpEstimator : NtcpMaximumLikelihoodEstimator<IPlanItem, LkbNtcpParameters>
    {
        public Func<IPlanItem, string> StructureSelector { get; set; }
        public DoseValue AlphaOverBeta { get; }


        public LkbNtcpEstimator(DoseValue alphaOverBeta)
        {
            AlphaOverBeta = alphaOverBeta;
            
            //if (base._parameterMapper == null)
            //{
            //    base._parameterMapper = new D50GammaTcpParameters();
            //}
        }


        protected override IOptimizer CreateSolver(int parameterCount)
        {
            return new SimplexGlobalOptimizer();
        }

        protected override (bool isNeeded, double penalityValue) Penalize(LkbNtcpParameters parameters)
        {
            return (false, double.NaN);
        }

        protected override double[] GetInitialParameters() => new[] { 50.0, 0.2, 0.5 };

        protected override double[] GetLowerBounds() => new[] { 1.0, 0.01, 0.01 };
        protected override double[] GetUpperBounds() => new[] { 200.0, 1.0, 1.0 };

        protected override LkbNtcpParameters ConvertVectorToParameters(double[] x) => new LkbNtcpParameters()
        {
            TD50 = x[0],
            M = x[1],
            N = x[2]
        };

        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? negLogLik)
        {
            // You can implement a real Hessian-based method here
            return new double[optimizedParams.Length];
        }

        protected  double ComputeNtcp(LkbNtcpParameters parameters, (double EUD, double Volume) data)
        {
            double eud = data.EUD;
            double td50 = parameters.TD50;
            double m = parameters.M;

            double t = (eud - td50) / (m * td50);
            return 0.5 * (1.0 + MathUtils.Erf(t / Math.Sqrt(2.0)));
        }

        public override double ComputeNtcp(LkbNtcpParameters parameters, IPlanItem data)
        {
            double td50 = parameters.TD50;
            double m = parameters.M;
            double alphaVolumeEffect = 1.0 / parameters.N;

            double eud = CalculateGeud(data, alphaVolumeEffect);
            double t = (eud - td50) / (m * td50);
            return 0.5 * (1.0 + MathUtils.Erf(t / Math.Sqrt(2.0)));
        }

        protected virtual double CalculateGeud(IPlanItem plan, double alphaVolumeEffect)
        {
            var structureId = StructureSelector(plan);
            if (string.IsNullOrWhiteSpace(structureId))
                throw new InvalidDataException("StructureId is missing!");

            var geudModel = Geud2GyModel.Create(alphaVolumeEffect);

            var cloud = plan.CalculateEqd2DoseDistribution(structureId, AlphaOverBeta);

            if (cloud?.VoxelDoses is null || cloud.VoxelDoses.Count == 0)
                throw new InvalidDataException("Dose is missing!");

            var geudResult = geudModel.Calculate(cloud);
            return geudResult.Value;
        }
    }
}