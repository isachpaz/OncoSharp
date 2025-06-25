// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using NUnit.Framework;
using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Density;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.Radiobiology.TCP;
using System.Diagnostics;
using OncoSharp.Core.Quantities;

namespace OncoSharp.Radiobiology.Tests;

[TestFixture]
public class TcpPoissonDensityModelTests
{
    private TcpPoissonDensityModel _model;

    [SetUp]
    public void SetUp()
    {
        _model = new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e8), 0.23);
    }

    [Test]
    public void ComputeTcp_MultipleIdenticalDosePoints_EqualsConsolidatedSinglePoint()
    {
        var model = new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e8), 0.23);

        var cloudPoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10)),
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10)),
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10)),
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10)),
        };

        var cloudPointsHomogenous = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(40)),
        };

        var tcp = model.ComputeTcp(cloudPoints);
        var tcpHomogenous = model.ComputeTcp(cloudPointsHomogenous);

        Assert.That(tcp, Is.EqualTo(tcpHomogenous));
    }

    // Edge Cases
    [Test]
    public void ComputeTcp_EmptyDoseCloudPoints_ReturnsOne()
    {
        var emptyCloudPoints = new List<DoseCloudPoint<EQD0Value>>();
        var tcp = _model.ComputeTcp(emptyCloudPoints);
        Assert.That(tcp.Value, Is.EqualTo(1.0).Within(1e-10));
    }

    [Test]
    public void ComputeTcp_ZeroDose_ReturnsZero()
    {
        var cloudPoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(0.Gy_Eqd0(), VolumeValue.InCM3(10))
        };
        var tcp = _model.ComputeTcp(cloudPoints);
        Assert.That(tcp.Value, Is.EqualTo(0.0).Within(1e-10));
    }

    [Test]
    public void ComputeTcp_ZeroVolume_ReturnsOne()
    {
        var cloudPoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(0))
        };
        var tcp = _model.ComputeTcp(cloudPoints);
        Assert.That(tcp.Value, Is.EqualTo(1.0).Within(1e-10));
    }

    // Dose Response Behavior
    [Test]
    public void ComputeTcp_IncreasedDose_IncreasedTcp()
    {
        var lowDose = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(50.Gy_Eqd0(), VolumeValue.InCM3(10))
        };
        var highDose = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10))
        };

        var tcpLow = _model.ComputeTcp(lowDose);
        var tcpHigh = _model.ComputeTcp(highDose);

        Assert.That(tcpHigh, Is.GreaterThan(tcpLow));
    }

    [Test]
    public void ComputeTcp_IncreasedVolume_DecreasedTcp()
    {
        var smallVolume = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(5))
        };
        var largeVolume = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(20))
        };

        var tcpSmall = _model.ComputeTcp(smallVolume);
        var tcpLarge = _model.ComputeTcp(largeVolume);

        Assert.That(tcpSmall, Is.GreaterThan(tcpLarge));
    }

    // Model Parameter Sensitivity
    [Test]
    public void ComputeTcp_HigherCellDensity_LowerTcp()
    {
        var lowDensityModel = new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e7), 0.23);
        var highDensityModel = new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e9), 0.23);

        var cloudPoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10))
        };

        var tcpLowDensity = lowDensityModel.ComputeTcp(cloudPoints);
        var tcpHighDensity = highDensityModel.ComputeTcp(cloudPoints);

        Assert.That(tcpLowDensity, Is.GreaterThan(tcpHighDensity));
    }

    [Test]
    public void ComputeTcp_HigherAlphaBetaRatio_DifferentTcp()
    {
        var lowAlphaBeta = new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e8), 0.1);
        var highAlphaBeta = new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e8), 0.5);

        var cloudPoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10))
        };

        var tcpLow = lowAlphaBeta.ComputeTcp(cloudPoints);
        var tcpHigh = highAlphaBeta.ComputeTcp(cloudPoints);

        Assert.That(tcpLow, Is.Not.EqualTo(tcpHigh));
    }

    // Heterogeneous Dose Distributions
    [Test]
    public void ComputeTcp_HeterogeneousDoses_BetweenMinAndMaxHomogeneousTcp()
    {
        var heterogeneousPoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(10)),
            new DoseCloudPoint<EQD0Value>(150.Gy_Eqd0(), VolumeValue.InCM3(10))
        };

        var lowDoseHomogeneous = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(20))
        };

        var highDoseHomogeneous = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(150.Gy_Eqd0(), VolumeValue.InCM3(20))
        };

        var tcpHeterogeneous = _model.ComputeTcp(heterogeneousPoints);
        var tcpLowHomogeneous = _model.ComputeTcp(lowDoseHomogeneous);
        var tcpHighHomogeneous = _model.ComputeTcp(highDoseHomogeneous);

        Assert.That(tcpHeterogeneous.Value, Is.GreaterThan(tcpLowHomogeneous.Value));
        Assert.That(tcpHeterogeneous.Value, Is.LessThan(tcpHighHomogeneous.Value));
    }

    // Mathematical Properties
    [Test]
    public void ComputeTcp_AdditiveVolumes_SameAsConsolidated()
    {
        var separatePoints = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(5)),
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(7)),
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(3))
        };

        var consolidatedPoint = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(15))
        };

        var tcpSeparate = _model.ComputeTcp(separatePoints);
        var tcpConsolidated = _model.ComputeTcp(consolidatedPoint);

        Assert.That(tcpSeparate.Value, Is.EqualTo(tcpConsolidated.Value).Within(1e-10));
    }

    // Boundary Conditions
    [Test]
    public void ComputeTcp_VeryHighDose_ApproachesOne()
    {
        var veryHighDose = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(1000.Gy_Eqd0(), VolumeValue.InCM3(1))
        };

        var tcp = _model.ComputeTcp(veryHighDose);
        Assert.That(tcp.Value, Is.GreaterThan(0.99));
    }

    [Test]
    public void ComputeTcp_VeryLargeTumor_ApproachesZero()
    {
        var largeTumor = new List<DoseCloudPoint<EQD0Value>>()
        {
            new DoseCloudPoint<EQD0Value>(10.Gy_Eqd0(), VolumeValue.InCM3(1000))
        };

        var tcp = _model.ComputeTcp(largeTumor);
        Assert.That(tcp.Value, Is.LessThan(0.01));
    }

    // Input Validation
    //[Test]
    //public void Constructor_NegativeCellDensity_ThrowsException()
    //{
    //    Assert.Throws<ArgumentException>(() =>
    //        new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(-1e8), 0.23));
    //}

    //[Test]
    //public void Constructor_NegativeAlphaBetaRatio_ThrowsException()
    //{
    //    Assert.Throws<ArgumentException>(() =>
    //        new TcpPoissonDensityModel(CellDensity.InCells_Per_CM3(1e8), -0.23));
    //}

    [Test]
    public void ComputeTcp_NullInput_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _model.ComputeTcp(null));
    }

    //[Test]
    //public void ComputeTcp_NegativeDose_ThrowsException()
    //{
    //    QuantityValidationConfig.EnforcePositive = false;

    //    var negativePoints = new List<DoseCloudPoint<EQD0Value>>()
    //    {
    //        new DoseCloudPoint<EQD0Value>(EQD0Value.InGy(-10), VolumeValue.InCM3(10))
    //    };

    //    Assert.Throws<ArgumentException>(() => _model.ComputeTcp(negativePoints));
    //}

    //[Test]
    //public void ComputeTcp_NegativeVolume_ThrowsException()
    //{
    //    QuantityValidationConfig.EnforcePositive = false;

    //    var negativePoints = new List<DoseCloudPoint<EQD0Value>>()
    //    {
    //        new DoseCloudPoint<EQD0Value>(100.Gy_Eqd0(), VolumeValue.InCM3(-10))
    //    };

    //    Assert.Throws<ArgumentException>(() => _model.ComputeTcp(negativePoints));
    //}

    // Performance/Numerical Stability
    [Test]
    public void ComputeTcp_LargeNumberOfPoints_ComputesEfficiently()
    {
        var manyPoints = new List<DoseCloudPoint<EQD0Value>>();
        for (int i = 0; i < 5_000_000; i++)
        {
            manyPoints.Add(new DoseCloudPoint<EQD0Value>(150.Gy_Eqd0(), VolumeValue.InCM3(0.01)));
        }

        var stopwatch = Stopwatch.StartNew();
        var tcp = _model.ComputeTcp(manyPoints);
        stopwatch.Stop();

        Assert.That(tcp.Value, Is.GreaterThan(0.0).And.LessThan(1.0));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000)); // Should complete in under 1 second
    }
}