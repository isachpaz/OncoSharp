// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Factories;
using OncoSharp.DVH.Helpers;
using OncoSharp.DVH.Metrics;

namespace OncoSharp.DVH.Tests
{
    [TestFixture]
    public class DVHMetricsTests
    {
        [TestCase(2, 31.12)]
        [TestCase(25, 23)]
        [TestCase(50, 19)]
        [TestCase(52, 16.6)]
        [TestCase(58, 14)]
        [TestCase(68, 12.4)]
        [TestCase(75, 8)]
        [TestCase(99, 2.24)]
        public void Dx_Small_Sample_Raw_Data_CDVH_Test(double quantileInPercent, double expectedValue)
        {
            var dosesArray = new List<double>
            {
                34, 26, 24, 19, 7,
                28, 24, 22, 14, 6,
                24, 23, 22, 14, 6,
                19, 21, 14, 8, 3,
                19, 14, 9, 8, 2
            };

            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, 1.cm3(), DoseUnit.Gy, 5);
            var mc = DVHMetricCalculator.Factory.QuantileType7(cdvh);

            var res = mc.GetDoseAtVolume(VolumeValue.InPercent(quantileInPercent));
            Assert.That(res, Is.EqualTo(expectedValue).Within(1e-2));
        }

        [TestCase(2, 31.12)]
        [TestCase(25, 23)]
        [TestCase(50, 19)]
        [TestCase(52, 16.6)]
        [TestCase(58, 14)]
        [TestCase(68, 12.4)]
        [TestCase(75, 8)]
        [TestCase(99, 2.24)]
        public void Dx_Small_Sample_Raw_Data_CDVH_Normalized_Test(double quantileInPercent, double expectedValue)
        {
            var dosesArray = new List<double>
            {
                34, 26, 24, 19, 7,
                28, 24, 22, 14, 6,
                24, 23, 22, 14, 6,
                19, 21, 14, 8, 3,
                19, 14, 9, 8, 2
            };

            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, 1.cm3(), DoseUnit.Gy, 5);
            var normalizedCDVH = cdvh.NormalizeVolume();
            var mc = DVHMetricCalculator.Factory.QuantileType7(normalizedCDVH);

            var res = mc.GetDoseAtVolume(VolumeValue.InPercent(quantileInPercent));
            Assert.That(res, Is.EqualTo(expectedValue).Within(1e-2));
        }


        [TestCase(2, 75.2872)]
        [TestCase(25, 71.4625)]
        [TestCase(50, 69.875)]
        [TestCase(52, 69.7468)]
        [TestCase(58, 69.5264)]
        [TestCase(68, 68.284)]
        [TestCase(75, 67.5325)]
        [TestCase(99, 62.0965)]
        public void Dx_Gaussian_Sample_CDVH_Test(double quantileInPercent, double expectedValue)
        {
            var samples = new double[]
            {
                73.27, 68.25, 70.74, 67.51, 72.69, 70.41, 72.27, 71.71, 69.56, 68.89,
                64.57, 65.44, 69.67, 68.30, 73.41, 67.47, 64.95, 69.62, 69.85, 70.34,
                68.91, 68.49, 68.59, 69.79, 69.94, 67.33, 72.37, 70.32, 74.29, 73.50,
                70.59, 65.71, 75.28, 68.75, 69.22, 62.87, 73.52, 70.48, 71.17, 74.91,
                72.68, 68.20, 70.35, 73.59, 65.52, 75.64, 67.77, 70.93, 66.66, 73.61,
                70.45, 69.69, 71.50, 69.38, 66.83, 69.63, 70.53, 67.19, 67.44, 66.78,
                65.24, 69.33, 72.26, 76.80, 67.23, 74.73, 70.47, 67.42, 71.45, 66.31,
                70.31, 67.65, 70.23, 73.17, 73.15, 69.90, 66.67, 69.98, 61.75, 71.23,
                62.10, 74.10, 69.48, 67.33, 66.71, 71.19, 67.66, 72.37, 70.31, 71.32,
                67.95, 69.97, 65.94, 72.49, 69.70, 70.24, 67.54, 74.01, 63.85, 70.05
            };

            var cdvh = CDVHFactory.FromDoseMatrix(samples.ToList(), 1.cm3(), DoseUnit.Gy, 0.1);
            var mc = DVHMetricCalculator.Factory.QuantileType7(cdvh);

            var dx = mc.GetDoseAtVolume(VolumeValue.InPercent(quantileInPercent));
            Assert.That(dx, Is.EqualTo(expectedValue).Within(1e-2));
        }
    }
}