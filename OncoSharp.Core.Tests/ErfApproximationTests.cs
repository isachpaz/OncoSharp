// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using MathNet.Numerics;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Helpers.Maths;

namespace OncoSharp.Core.Tests;

[TestFixture]
public class ErfApproximationTests
{
    public class ErfTests
    {
        private static readonly double[] TestValues = new double[]
        {
            -5.0, -3.0, -1.5, -1.0, -0.5,
            0.0,
            0.5, 1.0, 1.5, 3.0, 5.0
        };

        [TestCaseSource(nameof(TestValues))]
        public void TestErfApproximationAccuracy(double x)
        {
            double expected = SpecialFunctions.Erf(x);
            double actual = MathUtils.Erf(x);

            double absError = Math.Abs(expected - actual);
            //Assert.Less(absError, 1e-6, $"Abs error too high at x={x}: {absError}");
            Assert.That(absError, Is.LessThan(1e-6), $"Abs error too high at x={x}: {absError}");
        }

        [TestCaseSource(nameof(TestValues))]
        public void TestErfOddSymmetry(double x)
        {
            double actual = MathUtils.Erf(x);
            double actualNeg = MathUtils.Erf(-x);
            Assert.That(-actual, Is.EqualTo(actualNeg).Within(1e-6), $"Odd symmetry failed at x={x}");
        }

        [Test]
        public void TestErfZero()
        {
            Assert.That(0.0, Is.EqualTo(MathUtils.Erf(0.0)).Within(1e-6), "Erf(0) should be 0");
        }

        [Test]
        public void TestErfApproachesOne()
        {
            Assert.That(1.0, Is.EqualTo(MathUtils.Erf(10.0)).Within(1e-6), "Erf(10) should be ≈ 1");
            Assert.That(-1.0, Is.EqualTo(MathUtils.Erf(-10.0)).Within(1e-6), "Erf(-10) should be ≈ -1");
        }

    }
}