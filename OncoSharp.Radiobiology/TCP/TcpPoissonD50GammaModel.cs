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
using System.Linq;

namespace OncoSharp.Radiobiology.TCP
{
    public class TcpPoissonD50GammaModel
    {
        public double D50 { get; }
        public double Gamma { get; }
        public CellDensity Density { get; }
        public double Alpha { get; }

        public TcpPoissonD50GammaModel(double d50, double gamma)
        {
            D50 = d50;
            Gamma = gamma;
        }

        public virtual ProbabilityValue ComputeVoxelResponse(DoseCloudPoint<EQD2Value> dosePoint)
        {
            var volume = dosePoint.Volume;
            var eqd2 = dosePoint.Dose;


            var egamma = (Math.E * Gamma);
            var lnln2 = Math.Log(Math.Log(2.0));
            double egamma_minus_lnln2 = egamma - lnln2;
            var eqd2_devided_by_d50 = eqd2.Value / D50;
            var response = Math.Exp(-Math.Exp(egamma - (eqd2_devided_by_d50 * egamma_minus_lnln2)));

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
                    var voxelResponse = ComputeVoxelResponse(point);
                    tcp *= Math.Pow(voxelResponse.Value, volumeFraction);
                }
            }

            return tcp;
        }
    }
}