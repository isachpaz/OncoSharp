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

namespace OncoSharp.DVH
{
    public abstract class DVHBase : IEquatable<DVHBase>, IDVHBase
    {
        public IReadOnlyList<double> RawDoseSamples { get; internal set; } = new List<double>();
        public List<DVHPoint> DVHCurve { get; internal set; } = new List<DVHPoint>();
        public DVHSourceType DVHSourceType { get; internal set; } = DVHSourceType.Undefined;
        public string Id { get; }
        public DoseUnit DoseUnit { get; }
        public VolumeUnit VolumeUnit { get; }
        public double BinWidth { get; }
        public uint NumBins { get; }
        public bool IsRawDoseSamplesAvailable => RawDoseSamples.Any();

        public double MaxDose { get; internal set; }
        public double MinDose { get; internal set; }
        public VolumeValue TotalVolume { get; internal set; }
        public bool IsNormalized { get; set; } = false;

        public CDVH ToCumulative()
        {
            return (CDVH)this;
        }

        public DDVH ToDifferential()
        {
            return (DDVH)this;
            throw new NotImplementedException();
        }

        protected DVHBase(string id, DoseUnit doseUnit, VolumeUnit volumeUnit, uint numBins, double binWidth,
            DVHSourceType dvhSourceType = DVHSourceType.DoseMatrix)
        {
            Id = id;
            DoseUnit = doseUnit;
            VolumeUnit = volumeUnit;
            NumBins = numBins;
            BinWidth = binWidth;
            DVHSourceType = dvhSourceType;
        }

        public bool Equals(DVHBase other)
        {
            var b = DVHCurve.SequenceEqual(other.DVHCurve);

            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return DoseUnit == other.DoseUnit &&
                   VolumeUnit == other.VolumeUnit &&
                   BinWidth.Equals(other.BinWidth) &&
                   NumBins == other.NumBins &&
                   MaxDose.Equals(other.MaxDose) &&
                   MinDose.Equals(other.MinDose) &&
                   TotalVolume.Equals(other.TotalVolume) &&
                   DVHCurve.SequenceEqual(other.DVHCurve);
        }

        public override bool Equals(object obj)
        {
            return obj is DVHBase other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                foreach (var point in DVHCurve)
                    hashCode = (hashCode * 31) ^ point.GetHashCode();

                hashCode = (hashCode * 397) ^ (int)DoseUnit;
                hashCode = (hashCode * 397) ^ (int)VolumeUnit;
                hashCode = (hashCode * 397) ^ BinWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)NumBins;
                hashCode = (hashCode * 397) ^ MaxDose.GetHashCode();
                hashCode = (hashCode * 397) ^ MinDose.GetHashCode();
                hashCode = (hashCode * 397) ^ TotalVolume.GetHashCode();
                return hashCode;
            }
        }
    }
}