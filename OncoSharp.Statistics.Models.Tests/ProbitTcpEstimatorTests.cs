// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using NUnit.Framework;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Models.Tcp;

namespace OncoSharp.Statistics.Models.Tests;

[TestFixture]
public class ProbitTcpEstimatorTests
{

    [Test]
    public void Fit_ShouldEstimateReasonableParameters()
    {
        // Arrange
        var estimator = new ProbitTcpEstimator(DoseValue.InGy(10), 20);

        // Simulate patients: responders received 77 Gy, non-responders received 70 Gy
        var responders = new List<IPlanItem>
        {
            new MockPlanItem(77),
            new MockPlanItem(77),
            new MockPlanItem(78),
            new MockPlanItem(76),
            new MockPlanItem(80),
            new MockPlanItem(77),
            new MockPlanItem(75),
            new MockPlanItem(80),
        };

        var nonResponders = new List<IPlanItem>
        {
            new MockPlanItem(70),
            new MockPlanItem(71),
            new MockPlanItem(73),
            new MockPlanItem(69),
            new MockPlanItem(68),
            new MockPlanItem(72),
            new MockPlanItem(72),
            new MockPlanItem(74),
            
        };

        var plans = new List<IPlanItem>();
        plans.AddRange(responders);
        plans.AddRange(nonResponders);

        var observations = new List<bool> { true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, };

        // Act
        var result = estimator.Fit(observations, plans, mleResult =>
        {
            Console.WriteLine($"{mleResult.LogLikelihood} => Param: {mleResult.Parameters}");
        });

        // Assert
        Assert.That(result.Parameters.D50, Is.InRange(0, 200), "D50 out of expected range");
        Assert.That(result.Parameters.Gamma50, Is.InRange(0, 30), "Gamma50 out of expected range");
        Assert.That(result.Parameters.AlphaVolumeEffect, Is.InRange(-100, 100), "AlphaVolumeEffect out of expected range");
    }
}