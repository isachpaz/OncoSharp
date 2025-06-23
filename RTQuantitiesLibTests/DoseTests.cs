// // RTToolkitSharp
// // Copyright (c) 2014 - 2025 Medical Innovation and Technology P.C.
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/RTToolkitSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Helpers;


namespace RTQuantitiesLibTests
{
    [TestFixture]
    public class DoseTests
    {
        [Test]
        public void DoseEquality_Test()
        {
            var doseSamples = DoseHelper.PhysicalDoseRange(start: 1.1, stop: 10.2, step: 0.5, bIncludeLastValue: true,
                unit: DoseUnit.cGy);
            DoseValue dose1 = DoseValue.InGy(101.25);
            DoseValue dose2 = DoseValue.InGy(101.25);
            Console.WriteLine($@"dose1 => {dose1}");
            Console.WriteLine($@"dose2 => {dose2}");

            if (dose1.Equals(dose2))
            {
                Console.WriteLine($@"Equals => dose1.Equals(dose2) [{dose1.Equals(dose2)}]");
            }

            if (dose1 == dose2)
            {
                Console.WriteLine($@"Equals => dose1==dose2 [{dose1 == dose2}]");
            }

            Assert.That(dose2, Is.EqualTo(dose2));
        }

        [Test]
        public void DoseSettings_Decimal_Default_ForGy__Test()
        {
            var dose = DoseValue.InGy(10.0);
            var doseString = dose.ValueAsString;
            Assert.That(doseString, Is.EqualTo("10.00 Gy"));
        }

        [Test]
        public void DoseSettings_Decimal_Default_ForPercent__Test()
        {
            var settings = new Mock<DoseConfig>();
            settings.Setup(s => s.Decimals(DoseUnit.PERCENT)).Returns(2);
            settings.Setup(s => s.Error()).Returns(1E-9);

            var dose = DoseValue.New(10.0, DoseUnit.PERCENT);
            var doseString = dose.ValueAsString;

            Assert.That(doseString, Is.EqualTo("10.00 %"));
        }
    }
}