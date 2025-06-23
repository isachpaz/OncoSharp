// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH
{
    public class CDVH : DVHBase
    {
        public CDVH(string id, DoseUnit doseUnit, VolumeUnit volumeUnit, uint numBins, double binWidth)
            : base(id, doseUnit, volumeUnit, numBins, binWidth)
        {
        }

        [Obsolete]
        public double GetDoseAtVolume_Obsolete(VolumeValue sampleVolume)
        {
            if (!DVHCurve.Any()) return 0.0;

            var sorted = DVHCurve.OrderByDescending(p => p.Dose).ToList();
            var cumVol = VolumeValue.New(0, base.VolumeUnit);
            for (int i = 0; i < sorted.Count; i++)
            {
                cumVol += sorted[i].Volume;
                if (sampleVolume <= cumVol)
                {
                    if (i == 0) return sorted[i].Dose;
                    var prevVol = cumVol - sorted[i].Volume;
                    double t = (sampleVolume - prevVol) / sorted[i].Volume;
                    return sorted[i - 1].Dose + t * (sorted[i].Dose - sorted[i - 1].Dose);
                }
            }

            return sorted.Last().Dose;
        }

        [Obsolete]
        public double GetVolumeAtDose_Obsolete(double dose)
        {
            return DVHCurve.Where(p => p.Dose >= dose).Sum(p => p.Volume.Value);
        }

        public double? GetBinWidth()
        {
            if (DVHCurve.Count < 2) return null;
            return DVHCurve[1].Dose - DVHCurve[0].Dose;
        }

        public new DDVH ToDifferential()
        {
            double binWidth = GetBinWidth() ?? 1.0;
            var differential = new List<DVHPoint>();

            for (int i = 0; i < DVHCurve.Count - 1; i++)
            {
                double doseLow = DVHCurve[i].Dose;
                double doseHigh = DVHCurve[i + 1].Dose;

                // Defensive check: ensure bin spacing is consistent
                if (Math.Abs(doseHigh - doseLow - binWidth) > 1e-6)
                    throw new InvalidOperationException(
                        $"Non-uniform bin width detected at bin {i}: Δ={doseHigh - doseLow}, expected={binWidth}");

                var volLow = DVHCurve[i].Volume;
                var volHigh = DVHCurve[i + 1].Volume;
                var binVolume = volLow - volHigh;

                double binCenter = doseLow + 0.5 * binWidth;

                differential.Add(new DVHPoint(binCenter, binVolume));
            }

            // Add final bin (lowest dose bin)
            var last = DVHCurve[DVHCurve.Count - 1];
            double finalBinCenter = last.Dose + 0.5 * binWidth;
            var finalBinVolume = last.Volume; // Since vol at next (nonexistent) bin is zero

            differential.Add(new DVHPoint(finalBinCenter, finalBinVolume));

            return new DDVH(Id + "_differential", DoseUnit, VolumeUnit, (uint)DVHCurve.Count, binWidth)
            {
                DVHCurve = differential,
                MaxDose = this.MaxDose,
                MinDose = this.MinDose,
                TotalVolume = this.TotalVolume
            };
        }
    }
}