// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Diagnostics;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH
{
    [DebuggerDisplay("Dose: {Dose}, Volume: {Volume}")]
    public readonly struct DVHPoint : IEquatable<DVHPoint>
    {
        public double Dose { get; }
        public VolumeValue Volume { get; }

        public DVHPoint(double dose, VolumeValue volume)
        {
            Dose = dose;
            Volume = volume;
        }

        //public override string ToString()
        //{
        //    return $"{nameof(Dose)}: {Dose}, {nameof(Volume)}: {Volume}";
        //}

        public bool Equals(DVHPoint other)
        {
            return Dose.Equals(other.Dose) && Volume.Equals(other.Volume);
        }

        public override bool Equals(object obj)
        {
            return obj is DVHPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Dose.GetHashCode() * 397) ^ Volume.GetHashCode();
            }
        }
    }
}