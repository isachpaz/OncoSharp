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

namespace OncoSharp.DVH.Tests
{
    [TestFixture]
    public class DVHConversionTests
    {
        private List<double> dosesArray = new List<double>();

        [SetUp]
        public void SetUp()
        {
            dosesArray = new List<double>
            {
                34, 26, 24, 19, 7,
                28, 24, 22, 14, 6,
                24, 23, 22, 14, 6,
                19, 21, 14, 8, 3,
                19, 14, 9, 8, 2
            };
        }

        [Test]
        public void Convert2DDVH_Test()
        {
            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.345.cm3(), DoseUnit.Gy, binWidth: 5);
            var toDDVH = cdvh.ToDifferential();

            var expectedDDVH = DDVHFactory.FromDoseMatrix(dosesArray, 0.345.cm3(), DoseUnit.Gy, binWidth: 5);

            Assert.That(toDDVH, Is.EqualTo(expectedDDVH));
        }

        [Test]
        public void Convert2CDVH_Test()
        {
            var ddvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.345.cm3(), DoseUnit.Gy, binWidth: 5);
            var expectedCDVH = CDVHFactory.FromDoseMatrix(dosesArray, 0.345.cm3(), DoseUnit.Gy,
                binWidth: 5);
            var toCDVH = ddvh.ToCumulative();

            Assert.That(toCDVH, Is.EqualTo(expectedCDVH));
        }

        [Test]
        public void CDVH2VolumeNormalization_Test()
        {
            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.345.cm3(), DoseUnit.Gy, binWidth: 5);
            var toCDVH = cdvh.NormalizeVolume();

            foreach (var (normalizedPoint, originalPoint) in
                     toCDVH.DVHCurve.Zip(cdvh.DVHCurve, (point1, point2) => (point1, point2)))
            {
                Assert.That(normalizedPoint, Is.EqualTo(
                    new DVHPoint(originalPoint.Dose,
                        VolumeValue.InPercent(100.0 * originalPoint.Volume.Value / cdvh.TotalVolume.Value))));
            }
        }

        [Test]
        public void DDVH2VolumeNormalization_Test()
        {
            var ddvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.345.cm3(), DoseUnit.Gy, binWidth: 5);
            var toCDVH = ddvh.NormalizeVolume();

            foreach (var (normalizedPoint, originalPoint) in
                     toCDVH.DVHCurve.Zip(ddvh.DVHCurve, (point1, point2) => (point1, point2)))
            {
                Assert.That(normalizedPoint, Is.EqualTo(
                    new DVHPoint(originalPoint.Dose,
                        VolumeValue.InPercent(100.0 * originalPoint.Volume.Value / ddvh.TotalVolume.Value))));
            }
        }
    }
}