// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Density;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Optimization.Abstractions.Interfaces;
using OncoSharp.Optimization.Algorithms.Simplex;
using OncoSharp.Radiobiology.TCP;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using OncoSharp.Statistics.Models.Tcp.Parameters;
using System;
using System.Collections.Generic;

namespace OncoSharp.Statistics.Models.Tcp
{
    //
    public class PoissonTcpEstimator : TcpMaximumLikelihoodEstimator<IPlanItem, PoissonTcpParameters>
    {
        /// <summary>
        /// Function to extract the structure ID from the given plan item.
        /// Default is a constant "GTV".
        /// </summary>
        public Func<IPlanItem, string> StructureSelector { get; set; } = _ => "GTV";


        public PoissonTcpEstimator(DoseValue alphaOverBeta, int numberOfMultipleStarts)
        {
            AlphaOverBeta = alphaOverBeta;
            NumberOfMultipleStarts = numberOfMultipleStarts;
            if (base._parameterMapper == null)
            {
                base._parameterMapper = new PoissonTcpParameters();
            }
        }

        public int NumberOfMultipleStarts { get; set; }

        public DoseValue AlphaOverBeta { get; set; }


        protected override double[] CalculateStandardErrors(double[] optimizedParams, double? negLogLik)
        {
            // Placeholder: should use numerical Hessian or observed Fisher Information Matrix
            return new double[] { Double.NaN, Double.NaN};
        }


        protected override IOptimizer CreateSolver(int parameterCount)
        {
            return new SimplexGlobalOptimizer(numberOfMultipleStarts: NumberOfMultipleStarts);
        }

        protected override (bool isNeeded, double penalityValue) Penalize(PoissonTcpParameters parameters)
        {
            // parameters here are ALWAYS physical (after ConvertVectorToParameters)
            if (parameters.D50 <= 0.0) return (true, BadLL);
            if (parameters.Gamma50 < 0.0) return (true, BadLL); // or <= 0 if you require strictly positive

            return (false, double.NaN);
        }

        protected override double[] GetInitialParameters()
        {
            return new double[] { 0.12, 1 }; // Alpha, Beta, Log10ClonogenDensity
        }

        protected override double[] GetLowerBounds()
        {
            return new double[] { 0.12, 0 }; // reasonable biological bounds
        }

        protected override double[] GetUpperBounds()
        {
            return new double[] { 0.12, 10 };
        }

        protected override PoissonTcpParameters ConvertVectorToParameters(double[] x)
        {
            return new PoissonTcpParameters
            {
                Alpha = x[0],
                Log10ClonogenDensity = x[1]
            };
        }

        public override double ComputeTcp(PoissonTcpParameters parameters, IPlanItem planItem)
        {
            var model = new TcpPoissonDensityModel(
                CellDensity.InCells_Per_CM3(Math.Pow(10,parameters.Log10ClonogenDensity)),
                parameters.Alpha);

            var structureId = StructureSelector(planItem);
            var points = planItem.CalculateEqd0DoseDistribution(structureId, AlphaOverBeta);

            var tcp = model.ComputeTcp(points.VoxelDoses);
            return tcp.Value;
            
        }
    }
}