// // RTToolkitSharp
// // Copyright (c) 2014 - 2025 Medical Innovation and Technology P.C.
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/RTToolkitSharp for more information.

using NUnit.Framework;
using System;
using OncoSharp.Core.Quantities;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;

namespace RTQuantitiesLibTests
{
    [TestFixture]
    public class DoseValueTests
    {
        [Test]
        public void Add_Test()
        {
            var d1 = DoseValue.InGy(10.23);
            var d2 = DoseValue.InGy(10.77);

            var sum = d1 + d2;
            var sum1 = d1.Add(d2);
            var sum2 = d2.Add(d1);
            Assert.That(sum, Is.EqualTo(sum1));
            Assert.That(sum1, Is.EqualTo(sum2));
            Assert.That(sum.Value, Is.EqualTo(10.23 + 10.77));
        }

        [Test]
        public void Subtraction_Test()
        {
            var d1 = DoseValue.InGy(20.23);
            var d2 = DoseValue.InGy(10.77);

            var sum = d1 - d2;
            var sum1 = d1.Subtract(d2);
            Assert.That(sum, Is.EqualTo(sum1));
            Assert.That(sum.Value, Is.EqualTo(20.23 - 10.77));
        }

        [Test]
        public void DoseValue_Must_Positive_Or_Throw_Exception_Test()
        {
            QuantityValidationConfig.EnforcePositive = true;
            Assert.Throws<ArgumentOutOfRangeException>(() => DoseValue.InGy(-10.23));

            Assert.Throws<ArgumentOutOfRangeException>(() => DoseValue.New(-0.00012, DoseUnit.cGy));

            Assert.Throws<ArgumentOutOfRangeException>(() => DoseValue.New(-0.00012, DoseUnit.PERCENT));
            Assert.Throws<ArgumentOutOfRangeException>(() => DoseValue.New(-0.00012, DoseUnit.UNKNOWN));
        }

        [Test]
        public void Disable_Positive_Value_validation_Test()
        {
            QuantityValidationConfig.EnforcePositive = false;
            var d = (-23.45).Gy();

            Assert.That(d.Value, Is.Negative);
        }
    }
}