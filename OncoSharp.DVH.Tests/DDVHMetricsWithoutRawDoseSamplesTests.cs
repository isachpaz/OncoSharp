// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Linq;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Metrics;
using OncoSharp.DVH.Parsers.DVHLib.Parsers;

namespace OncoSharp.DVH.Tests
{
    [TestFixture]
    public class DDVHMetricsWithoutRawDoseSamplesTests
    {
        private DDVH _ddvh;

        [SetUp]
        public void SetUp()
        {
            var dvh = DDVHParser.Parse(@"Input\DDVH_HN1.txt");
            var hn1Entries = dvh.Structures.First(x => x.Name == "HN1");
            _ddvh = (DDVH)dvh.DVHs.First();
        }


        [TestCase(2, 52.605)]
        [TestCase(50, 50.101)]
        [TestCase(66, 49.559)]
        [TestCase(90, 47.220)]
        [TestCase(98, 38.686)]
        public void Dx_Case_1_DDVH_DxPercent_Test(double quantileInPercent, double expectedValue)
        {
            var mc = DVHMetricCalculator.Factory.QuantileType7(_ddvh.ToCumulative());

            var cdvh = _ddvh.ToCumulative();
            var dx = mc.GetDoseAtVolume(VolumeValue.InPercent(quantileInPercent));
            Assert.That(dx, Is.EqualTo(expectedValue).Within(1e-2));
        }


        [TestCase(2, 54.301)]
        [TestCase(0.1, 55.391)]
        public void Dx_Case_1_DDVH_DxCC_Test(double quantileInCC, double expectedValue)
        {
            var mc = DVHMetricCalculator.Factory.QuantileType7(_ddvh.ToCumulative());

            var dx = mc.GetDoseAtVolume(quantileInCC.cm3());
            Assert.That(dx, Is.EqualTo(expectedValue).Within(1e-2));
        }
    }
}