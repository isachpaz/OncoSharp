// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Probability;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OncoSharp.Radiobiology.TCP
{
    public class TcpLogisticModel
    {
        public double D50 { get; }
        public double Gamma50 { get; }


        public TcpLogisticModel(double d50, double gamma50)
        {
            D50 = d50;
            Gamma50 = gamma50;
        }

        public virtual ProbabilityValue ComputeVoxelResponse(EQD2Value eqd2)
        {
            var response = 1.0 + Math.Exp(4.0 * Gamma50 * (1.0 - eqd2.Value / D50));
            response = 1.0 / response;

            if (Double.IsNaN(response))
            {
                Debug.WriteLine("ComputeVoxelResponse was NaN. Please, check further!");
                response = 0.0;
            }

            return ProbabilityValue.New(response);
        }

        public virtual ProbabilityValue ComputeTcp(List<DoseCloudPoint<EQD2Value>> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            ProbabilityValue tcp = ProbabilityValue.One;
            var totalVolume = points.Select(p => p.Volume.Value).Sum();

            foreach (var point in points)
            {
                var dose = point.Dose;
                var volume = point.Volume;
                var volumeFraction = volume.Value / totalVolume;

                if (Math.Abs(volumeFraction) < 1E-16)
                {
                    tcp *= 1.0;
                }
                else
                {
                    var voxelResponse = ComputeVoxelResponse(point.Dose);
                    tcp *= Math.Pow(voxelResponse.Value, volumeFraction);
                }
            }

            return tcp;
        }
    }
}