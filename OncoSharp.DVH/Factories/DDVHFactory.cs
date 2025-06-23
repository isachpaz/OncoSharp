// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH.Factories
{
    public class DDVHFactory
    {
        public static DDVH FromDoseMatrix(
            List<double> dosesInStructure,
            VolumeValue voxelVolume,
            DoseUnit doseUnit,
            double binWidth = 0.01,
            string id = "")
        {
            if (voxelVolume.Unit == VolumeUnit.PERCENT || voxelVolume.Unit == VolumeUnit.UNKNOWN)
                throw new InvalidEnumArgumentException("VoxelVolume must be either cm³ or mm³");

            var totalVolume = dosesInStructure.Count * voxelVolume;

            double maxDose = dosesInStructure.Max();
            int numBins = (int)Math.Ceiling(maxDose / binWidth) + 1; // Include final bin

            int[] binCounts = new int[numBins];

            // Count doses into bins
            foreach (var dose in dosesInStructure)
            {
                int binIndex = (int)(dose / binWidth);
                binIndex = Math.Min(binIndex, numBins - 1); // clamp to last bin
                binCounts[binIndex]++;
            }

            // Convert to differential DVH (volume per bin)
            List<DVHPoint> differentialDVH = new List<DVHPoint>(numBins);
            int totalCount = dosesInStructure.Count;

            for (int i = 0; i < numBins; i++)
            {
                double binCenter = (i + 0.5) * binWidth;
                VolumeValue volume = binCounts[i] * voxelVolume; // cm³ 
                //: binCounts[i] * voxelVolume / binWidth; // cm³ / Gy

                differentialDVH.Add(new DVHPoint(binCenter, volume));
            }

            var dDvh = new DDVH(id, doseUnit, voxelVolume.Unit, (uint)differentialDVH.Count, binWidth)
            {
                DVHCurve = differentialDVH,
                MaxDose = maxDose,
                MinDose = dosesInStructure.Min(),
                TotalVolume = totalVolume,
                RawDoseSamples = dosesInStructure,
                DVHSourceType = DVHSourceType.DoseMatrix
            };
            return dDvh;
        }

        public static DDVH FromDifferentialDVHPoints(List<DVHPoint> dvhPoints,
            double maxDose,
            double minDose,
            VolumeValue totalVolume,
            DoseUnit doseUnit,
            string id = "")
        {
            dvhPoints = dvhPoints.ToAbsoluteVolumeValue(totalVolume);
            double binWidth = Math.Abs(dvhPoints[0].Dose - dvhPoints[1].Dose);

            var dvh = new DDVH(id, doseUnit, totalVolume.Unit, (uint)dvhPoints.Count, binWidth)
            {
                DVHCurve = dvhPoints,
                MaxDose = maxDose,
                MinDose = minDose,
                TotalVolume = totalVolume,
                RawDoseSamples = new List<double>(),
                DVHSourceType = DVHSourceType.ExportedDVH
            };

            return dvh;
        }
    }
}