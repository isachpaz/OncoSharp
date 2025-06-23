// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using OncoSharp.Core.Quantities;
using OncoSharp.Core.Quantities.DimensionlessValues;
using OncoSharp.Core.Quantities.Probability;

namespace OncoSharp.Core.Tests
{
    [TestFixture]
    public class ProbabilityCreation_Tests
    {
        [Test]
        public void Is_Probability_Empty_Test()
        {
            var emptyProbability = ProbabilityValue.Empty();
            Assert.That(emptyProbability.IsEmpty, Is.True);
        }

        [Test]
        public void Probability_ctor_Not_Expected_Values_GreaterThan_One_Test()
        {
            //var doublesValuesSamples = RangeHelper<DoubleValue>.Range(
            //    1.001,
            //    100,
            //    0.1,
            //    true);

            //foreach (var item in doublesValuesSamples)
            //{
            //    Assert.Throws<ArgumentException>(
            //        () => { ProbabilityValue.New(item.Value); });
            //}
        }

        [Test]
        public void Probability_ctor_Not_Expected_Values_LessThan_Zero_Test()
        {
            //var doublesValuesSamples = RangeHelper<DoubleValue>.Range(
            //    -10,
            //    -0.00001,
            //    0.1,
            //    true);

            //foreach (var item in doublesValuesSamples)
            //{
            //    Assert.Throws<ArgumentException>(
            //        () => { ProbabilityValue.New(item.Value); });
            //}
        }


        [Test]
        public void ProbabilityEquality_Test()
        {
            var settings = new Mock<IQuantityConfig<UnitLess>>();
            settings.Setup(s => s.Decimals(UnitLess.UNITLESS)).Returns(3);
            settings.Setup(s => s.Error()).Returns(1e-9);

            Console.WriteLine("*************************************************************************");
            ProbabilityValue probability1 = ProbabilityValue.New(0.1, settings.Object);
            ProbabilityValue probability2 = ProbabilityValue.New(0.1, settings.Object);

            Assert.That(probability1, Is.EqualTo(probability2));
            Console.WriteLine("*************************************************************************");
        }


        [TestCase(0.1, 0.5, false)]
        [TestCase(0.5, 0.50005, false)]
        [TestCase(0.8, 0.5, true)]
        [TestCase(0.8, 0.79999, true)]
        [TestCase(1.0, 0.0, true)]
        [TestCase(0.77, 0.77, false)]
        public void Probability_Relational_Greater_Or_Less_Tests(
            double probValue1,
            double probValue2,
            bool isGreater)
        {
            Debug.WriteLine("Probability_Relational_Greater_Or_Less_Tests");

            var prob1 = ProbabilityValue.New(probValue1);
            var prob2 = ProbabilityValue.New(probValue2);

            if (prob1 > prob2)
            {
                Assert.That(isGreater, Is.True);
            }
            else
            {
                Assert.That(isGreater, Is.False);
            }
        }


        [TestCase(0.23, 0.23000001, false)]
        [TestCase(0.23, 0.2300000, true)]
        public void Probability_Relational_GreaterEqual_Tests(
            double probValue1,
            double probValue2,
            bool isGreaterOrEqual)
        {
            var prob1 = ProbabilityValue.New(probValue1);
            var prob2 = ProbabilityValue.New(probValue2);

            if (prob1 >= prob2)
            {
                Assert.That(isGreaterOrEqual, Is.True);
            }
            else
            {
                Assert.That(isGreaterOrEqual, Is.False);
            }
        }


        [Test]
        public void HowTo_Use_With_Custom_Settings_Test()
        {
            Debug.WriteLine("HowTo_Use_With_Custom_Settings_Test");

            var settings = new Mock<IQuantityConfig<UnitLess>>();
            settings.Setup(s => s.Decimals(UnitLess.UNITLESS)).Returns(3);

            var prob = ProbabilityValue.New(0.9, settings.Object);

            var probValueAsString = prob.ValueAsString;
            var expected = 0.9.ToString($"F{3:D}");
            Assert.That(probValueAsString, Is.EqualTo(expected));
        }


        [TestCase(0.5, 0.5, 1.0)]
        [TestCase(0.00001, 0.5, 0.50001)]
        public void Probability_Addition_Tests(double prob1Value, double prob2Value, double expectedValue)
        {
            Debug.WriteLine("Probability_Addition_Tests");

            var prob1 = ProbabilityValue.New(prob1Value);
            var prob2 = ProbabilityValue.New(prob2Value);

            var observed = prob1 + prob2;
            Assert.That(observed, Is.EqualTo(ProbabilityValue.New(expectedValue)));
        }

        [TestCase(0.5, 0.9)]
        [TestCase(0.4, 0.7)]
        public void Probability_Addition_Exception_Tests(double prob1Value, double prob2Value)
        {
            var prob1 = ProbabilityValue.New(prob1Value);
            var prob2 = ProbabilityValue.New(prob2Value);


            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var observed = prob1 + prob2;
            });
        }

        [TestCase(0.5, 0.5, 0.25)]
        [TestCase(0.1, 0.1, 0.01)]
        [TestCase(0.9, 0.9, 0.81)]
        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(0.0, 0.0, 0.0)]
        public void Probability_Multiplication_Tests(double prob1Value, double prob2Value, double expectedValue)
        {
            Debug.WriteLine("Probability_Multiplication_Tests");
            var prob1 = ProbabilityValue.New(prob1Value);
            var prob2 = ProbabilityValue.New(prob2Value);

            var observed = prob1 * prob2;
            var expected = ProbabilityValue.New(expectedValue);

            Assert.That(observed, Is.EqualTo(expected));
        }

        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        public void Probability_Ctor_Special_Numbers_Tests(double prob1Value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { ProbabilityValue.New(prob1Value); });
        }
    }
}