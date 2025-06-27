// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Density;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Probability;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OncoSharp.Radiobiology.TCP
{
    public class TcpPoissonDensityModel
    {
        public CellDensity Density { get; }
        public double Alpha { get; }

        public TcpPoissonDensityModel(CellDensity density, double alpha)
        {
            Density = density;
            Alpha = alpha;
        }

        public virtual ProbabilityValue ComputeVoxelResponse(DoseCloudPoint<EQD0Value> dosePoint)
        {
            var volume = dosePoint.Volume;
            var eqd0 = dosePoint.Dose;
            
            //if (volume.Value < 1e-10)
            //    return ProbabilityValue.One;

            double response = Math.Exp(-Density.Value * volume.Value * Math.Exp(-Alpha * eqd0.Value));

            if (Double.IsNaN(response))
            {
                Debug.WriteLine("ComputeVoxelResponse was NaN. Please, check further!");
                response = 0.0;
            }

            return ProbabilityValue.New(response);
        }

        public virtual ProbabilityValue ComputeTcp(List<DoseCloudPoint<EQD0Value>> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            ProbabilityValue tcp = ProbabilityValue.One;

            foreach (var point in points)
            {
                var voxelResponse = ComputeVoxelResponse(point);
                tcp *= voxelResponse.Value;
            }
            
            return tcp;
        }
    }
}