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
    public static class CDVHFactory
    {
        public static CDVH FromDoseMatrix(
            List<double> dosesInStructure,
            VolumeValue voxelVolume,
            DoseUnit doseUnit,
            double binWidth = 0.01,
            string id = "")
        {
            if (voxelVolume.Unit == VolumeUnit.PERCENT || voxelVolume.Unit == VolumeUnit.UNKNOWN)
                throw new InvalidEnumArgumentException("volumeUnit must be either cm³ or mm³");

            var totalVolume = dosesInStructure.Count * voxelVolume;

            double maxDose = dosesInStructure.Max();
            int numBins = (int)Math.Ceiling(maxDose / binWidth);
            int[] binCounts = new int[numBins + 1];

            // Step 1: Count voxels into bins [edge-aligned]
            foreach (var dose in dosesInStructure)
            {
                int binIndex = Math.Min((int)(dose / binWidth), numBins);
                binCounts[binIndex]++;
            }

            // Step 2: Build cumulative histogram from highest to lowest bin
            List<DVHPoint> cumulativeDVH = new List<DVHPoint>(numBins + 1);
            int cumulativeCount = 0;
            int totalCount = dosesInStructure.Count;

            for (int i = numBins; i >= 0; i--)
            {
                cumulativeCount += binCounts[i];
                double doseEdge = i * binWidth; // edge, not center
                var volume = cumulativeCount * voxelVolume;

                cumulativeDVH.Insert(0, new DVHPoint(doseEdge, volume));
            }

            var dvh = new CDVH(id, doseUnit, voxelVolume.Unit, (uint)cumulativeDVH.Count, binWidth)
            {
                DVHCurve = cumulativeDVH,
                MaxDose = maxDose,
                MinDose = dosesInStructure.Min(),
                TotalVolume = totalVolume,
                RawDoseSamples = dosesInStructure,
                DVHSourceType = DVHSourceType.DoseMatrix
            };

            return dvh;
        }

        public static CDVH FromCumulativeDVHPoints(
            List<DVHPoint> dvhPoints,
            double maxDose,
            double minDose,
            VolumeValue totalVolume,
            DoseUnit doseUnit,
            string id = "")
        {
            dvhPoints = dvhPoints.ToAbsoluteVolumeValue(totalVolume);

            double binWidth = Math.Abs(dvhPoints[0].Dose - dvhPoints[1].Dose);

            var dvh = new CDVH(id, doseUnit, totalVolume.Unit, (uint)dvhPoints.Count, binWidth)
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