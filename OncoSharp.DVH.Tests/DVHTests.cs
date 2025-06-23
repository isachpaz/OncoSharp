// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.DVH.Factories;

namespace OncoSharp.DVH.Tests
{
    [TestFixture]
    public class DvhTests
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

        [TestCase(1, 25)]
        [TestCase(2, 50)]
        [TestCase(0.1, 2.5)]
        [TestCase(0.01, 0.25)]
        public void CDVH_TotalVolume_Test(double voxelVolume, double totalVolume)
        {
            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, voxelVolume.cm3(), DoseUnit.Gy, binWidth: 5);
            Assert.That(cdvh.TotalVolume, Is.EqualTo(totalVolume.cm3()));
        }

        [TestCase(1, 25)]
        [TestCase(2, 50)]
        [TestCase(0.1, 2.5)]
        [TestCase(0.01, 0.25)]
        public void DDVH_TotalVolume_Test(double voxelVolume, double totalVolume)
        {
            var ddvh = CDVHFactory.FromDoseMatrix(dosesArray, voxelVolume.cm3(), DoseUnit.Gy, binWidth: 0.1);
            Assert.That(ddvh.TotalVolume, Is.EqualTo(totalVolume.cm3()));
        }

        [Test]
        public void CDVH_MinDose_Test()
        {
            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.001.cm3(), DoseUnit.Gy, binWidth: 5);
            Assert.That(cdvh.MinDose, Is.EqualTo(2.0));
        }

        [Test]
        public void DDVH_MinDose_Test()
        {
            var ddvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.001.cm3(), DoseUnit.Gy, binWidth: 5);
            Assert.That(ddvh.MinDose, Is.EqualTo(2.0));
        }


        [Test]
        public void CDVH_MaxDose_Test()
        {
            var cdvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.001.cm3(), DoseUnit.Gy, binWidth: 5);
            Assert.That(cdvh.MaxDose, Is.EqualTo(34.0));
        }

        [Test]
        public void DDVH_MaxDose_Test()
        {
            var ddvh = CDVHFactory.FromDoseMatrix(dosesArray, 0.001.cm3(), DoseUnit.Gy, binWidth: 5);
            Assert.That(ddvh.MaxDose, Is.EqualTo(34.0));
        }
    }
}