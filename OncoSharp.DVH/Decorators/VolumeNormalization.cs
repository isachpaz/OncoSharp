// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH.Decorators
{
    public class VolumeNormalization : IDVHBase
    {
        private readonly IDVHBase _inner;

        public VolumeNormalization(IDVHBase dvh)
        {
            _inner = dvh ?? throw new ArgumentNullException(nameof(dvh));
        }

        public IReadOnlyList<double> RawDoseSamples => _inner.RawDoseSamples;

        public List<DVHPoint> DVHCurve
        {
            get
            {
                var dvhPoints = _inner.DVHCurve;
                var normalizedPoints = dvhPoints.Select(p =>
                    new DVHPoint(p.Dose, VolumeValue.InPercent(100.0 * p.Volume / _inner.TotalVolume)));
                return normalizedPoints.ToList();
            }
        }


        public string Id => _inner.Id;

        public DoseUnit DoseUnit => _inner.DoseUnit;

        public VolumeUnit VolumeUnit => VolumeUnit.PERCENT;

        public double BinWidth => _inner.BinWidth;

        public uint NumBins => _inner.NumBins;

        public bool IsRawDoseSamplesAvailable => _inner.IsRawDoseSamplesAvailable;

        public double MaxDose => _inner.MaxDose;

        public double MinDose => _inner.MinDose;

        public VolumeValue TotalVolume => _inner.TotalVolume;

        public CDVH ToCumulative()
        {
            return _inner.ToCumulative();
        }

        public DDVH ToDifferential()
        {
            return _inner.ToDifferential();
        }

        public bool IsNormalized => true;
    }
}