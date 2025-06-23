// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Factories;
using OncoSharp.DVH.Helpers;

namespace OncoSharp.DVH.Tests
{
    [TestFixture]
    public class DDVH_Creation_Tests
    {
        private IDVHBase _difDVH;

        [SetUp]
        public void SetUp()
        {
            var random = new MersenneTwister(seed: 42);
            var normal = new Normal(mean: 70, stddev: 3, random);
            var samples = normal.Samples()
                .Take(100)
                .Select(x => Math.Round(x, 2))
                //.Select(x => new DVHPoint(x, 0.008.cm3()))
                .ToList();

            _difDVH = DDVHFactory.FromDoseMatrix(samples, 0.008.cm3(), DoseUnit.Gy, 0.5);
        }

        [Test]
        public void DDVH_From_DoseMatrix_VolumeAbsolute_Test()
        {
            var dosesArray = new List<double>
            {
                34, 26, 24, 19, 7,
                28, 24, 22, 14, 6,
                24, 23, 22, 14, 6,
                19, 21, 14, 8, 3,
                19, 14, 9, 8, 2
            };

            var ddvh = DDVHFactory.FromDoseMatrix(dosesArray, 0.001.cm3(), DoseUnit.Gy);
            Assert.That(ddvh.VolumeUnit, Is.EqualTo(VolumeUnit.CM3));
            Assert.That(ddvh.DVHSourceType, Is.EqualTo(DVHSourceType.DoseMatrix));
        }

        [Test]
        public void DDVH_From_DoseMatrix_VolumeRelative_Test()
        {
            var dosesArray = new List<double>
            {
                34, 26, 24, 19, 7,
                28, 24, 22, 14, 6,
                24, 23, 22, 14, 6,
                19, 21, 14, 8, 3,
                19, 14, 9, 8, 2
            };

            Assert.Throws<InvalidEnumArgumentException>(() =>
                DDVHFactory.FromDoseMatrix(dosesArray, new VolumeValue(.001, VolumeUnit.PERCENT), DoseUnit.Gy)
            );
        }

        [Test]
        public void DDVH_From_DVHPoints_VolumeAbsolute_Test()
        {
            var points = new List<DVHPoint>(_difDVH.DVHCurve);
            var ddvh = DDVHFactory.FromDifferentialDVHPoints(points, _difDVH.MaxDose, _difDVH.MinDose,
                _difDVH.TotalVolume, _difDVH.DoseUnit);

            Assert.That(ddvh.DVHSourceType, Is.EqualTo(DVHSourceType.ExportedDVH));
            Assert.That(ddvh.DVHCurve.First().Volume.Unit, Is.EqualTo(_difDVH.VolumeUnit));
        }

        [Test]
        public void DDVH_From_DVHPoints_VolumeRelative_Test()
        {
            var points = new List<DVHPoint>(_difDVH.NormalizeVolume().DVHCurve);
            var ddvh = DDVHFactory.FromDifferentialDVHPoints(points, _difDVH.MaxDose, _difDVH.MinDose,
                _difDVH.TotalVolume, _difDVH.DoseUnit);

            Assert.That(ddvh.DVHSourceType, Is.EqualTo(DVHSourceType.ExportedDVH));
            Assert.That(ddvh.DVHCurve.First().Volume.Unit, Is.EqualTo(_difDVH.VolumeUnit));
        }
    }
}