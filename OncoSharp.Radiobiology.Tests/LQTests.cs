using NUnit.Framework;
using OncoSharp.Radiobiology.LQ;

namespace OncoSharp.Radiobiology.Tests
{
    [TestFixture]
    public class LQTests
    {

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(35)]
        [TestCase(40)]
        [TestCase(45)]

        public void LQ_AlphaBetaRatioZero_Test(double numberOfFractions)
        {
            var dose = 100.0;
            var lq = new LqFractionated(0.0, numberOfFractions);
            var eqd2 = lq.ComputeEqd2(dose);
            var eqd0 = lq.ComputeEqd0(dose);

            var eqd2x = eqd0 / (1.0 + 2.0 / 0.0);
            var expectedEqd2 = dose * dose / 2.0 / numberOfFractions;

            Assert.That(eqd2, Is.EqualTo(expectedEqd2).Within(1e-6));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(35)]
        [TestCase(40)]
        [TestCase(45)]

        public void LQ_AlphaBetaRatioNearZero_Test(double numberOfFractions)
        {
            var lq = new LqFractionated(0.0001, numberOfFractions);
            var eqd2 = lq.ComputeEqd2(100);
            var eqd0 = lq.ComputeEqd0(100);

            var eqd2x = eqd0 / (1.0 + 2.0 / 0.0001);
            Assert.That(eqd2, Is.EqualTo(eqd2x).Within(0.3));
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(35)]
        [TestCase(40)]
        [TestCase(45)]

        public void LQ_AlphaBetaRatioLargePositive_Test(double numberOfFractions)
        {
            var lq = new LqFractionated(1e7, numberOfFractions);
            var eqd2 = lq.ComputeEqd2(100);
            var eqd0 = lq.ComputeEqd0(100);

            var eqd2x = eqd0 / (1.0 + 2.0 / 1e7);
            Assert.That(eqd2, Is.EqualTo(eqd2x).Within(0.001));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        [TestCase(30)]
        [TestCase(35)]
        [TestCase(40)]
        [TestCase(45)]

        public void LQ_AlphaBetaRatioLargeNegative_Test(double numberOfFractions)
        {
            var lq = new LqFractionated(-1e7, numberOfFractions);
            var eqd2 = lq.ComputeEqd2(100);
            var eqd0 = lq.ComputeEqd0(100);

            var eqd2x = eqd0 / (1.0 + 2.0 / -1e7);
            Assert.That(eqd2, Is.EqualTo(eqd2x).Within(0.001));
        }
    }
}
