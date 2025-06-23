// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH
{
    public interface IDVHBase
    {
        IReadOnlyList<double> RawDoseSamples { get; }
        List<DVHPoint> DVHCurve { get; }
        string Id { get; }
        DoseUnit DoseUnit { get; }
        VolumeUnit VolumeUnit { get; }
        double BinWidth { get; }
        uint NumBins { get; }
        bool IsRawDoseSamplesAvailable { get; }
        double MaxDose { get; }
        double MinDose { get; }
        VolumeValue TotalVolume { get; }
        CDVH ToCumulative();
        DDVH ToDifferential();
        bool IsNormalized { get; }
    }
}