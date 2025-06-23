// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH
{
    public class DDVH : DVHBase
    {
        public DDVH(string id, DoseUnit doseUnit, VolumeUnit volumeUnit, uint numBins, double binWidth)
            : base(id, doseUnit, volumeUnit, numBins, binWidth)
        {
        }

        public double? GetBinWidth()
        {
            if (DVHCurve.Count < 2) return null;
            return DVHCurve[1].Dose - DVHCurve[0].Dose;
        }

        // Optional: integrate to get cumulative DVH
        public new CDVH ToCumulative()
        {
            double binWidth = GetBinWidth() ?? 1.0; // Default to 1.0 if bin width is not defined
            var cumulative = new List<DVHPoint>();
            var totalVolume = VolumeValue.New(0.0, base.VolumeUnit);

            for (int i = DVHCurve.Count - 1; i >= 0; i--)
            {
                totalVolume += DVHCurve[i].Volume;

                // Convert bin center to bin edge (assuming DVHCurve[i].Dose is a center)
                double edgeDose = DVHCurve[i].Dose - 0.5 * binWidth;

                cumulative.Insert(0, new DVHPoint(edgeDose, totalVolume));
            }

            return new CDVH(Id + "_cumulative", DoseUnit, VolumeUnit, (uint)DVHCurve.Count, binWidth)
            {
                DVHCurve = cumulative,
                MaxDose = this.MaxDose,
                MinDose = this.MinDose,
                TotalVolume = totalVolume
            };
        }
    }
}