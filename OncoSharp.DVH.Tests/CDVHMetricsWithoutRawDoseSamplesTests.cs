// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Factories;
using OncoSharp.DVH.Metrics;
using OncoSharp.DVH.Parsers;

namespace OncoSharp.DVH.Tests
{
    [TestFixture]
    public class CDVHMetricsWithoutRawDoseSamplesTests
    {
        private CDVH _cdvh;

        [SetUp]
        public void SetUp()
        {
            var dvh = CDVHParser.Parse(@"Input\CDVH_HN1.txt");
            var hn1Entries = dvh.Structures.First(x => x.Name == "HN1");

            VolumeValue totalVolume = hn1Entries.VolumeCm3.cm3();
            List<DVHPoint> dvhPoints = new List<DVHPoint>();

            foreach (CDVHEntry item in hn1Entries.Entries)
            {
                dvhPoints.Add(new DVHPoint(item.DoseGy, VolumeValue.InPercent(item.VolumePercent)));
            }

            double maxDose = (double)Double.Parse(hn1Entries.Stats["Max Dose [Gy]"]);
            double minDose = (double)Double.Parse(hn1Entries.Stats["Min Dose [Gy]"]);
            _cdvh = CDVHFactory.FromCumulativeDVHPoints(dvhPoints, maxDose, minDose, totalVolume, DoseUnit.Gy);
        }


        [TestCase(2, 52.605)]
        [TestCase(50, 50.101)]
        [TestCase(66, 49.559)]
        [TestCase(90, 47.220)]
        [TestCase(98, 38.686)]
        public void Dx_Case_1_CDVH_DxPercent_Test(double quantileInPercent, double expectedValue)
        {
            var mc = DVHMetricCalculator.Factory.QuantileType7(_cdvh);

            var dx = mc.GetDoseAtVolume(VolumeValue.InPercent(quantileInPercent));
            Assert.That(dx, Is.EqualTo(expectedValue).Within(1e-2));

            var d2cc = mc.GetDoseAtVolume(2.0.cm3());
        }


        [TestCase(2, 54.301)]
        [TestCase(0.1, 55.391)]
        public void Dx_Case_1_CDVH_DxCC_Test(double quantileInCC, double expectedValue)
        {
            var mc = DVHMetricCalculator.Factory.QuantileType7(_cdvh);

            var dx = mc.GetDoseAtVolume(quantileInCC.cm3());
            Assert.That(dx, Is.EqualTo(expectedValue).Within(1e-2));
        }
    }
}