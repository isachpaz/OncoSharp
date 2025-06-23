// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;

namespace OncoSharp.Core.Quantities.Volume
{
    public readonly struct VolumeValue :
        IEquatable<VolumeValue>,
        IComparable<VolumeValue>,
        IFormattable
    {
        private readonly QuantityCore<VolumeUnit> _core;

        public double Value => _core.Value;
        public VolumeUnit Unit => _core.Unit;


        public VolumeValue(double value, VolumeUnit unit, IQuantityConfig<VolumeUnit> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));

            config = config ?? VolumeConfig.Default();

            _core = new QuantityCore<VolumeUnit>(checkedValue, unit,
                config.Decimals(default),
                config.Error());
        }

        public static VolumeValue New(double value, VolumeUnit unit) => new VolumeValue(value, unit);
        public static VolumeValue Empty() => new VolumeValue(double.NaN, VolumeUnit.UNKNOWN);

        public int CompareTo(VolumeValue other) => _core.CompareTo(other._core);
        public bool Equals(VolumeValue other) => _core.Equals(other._core);

        public override int GetHashCode() => _core.GetHashCode();

        public string ToString(string format, IFormatProvider formatProvider) =>
            $"{_core.Format(format, formatProvider)} {GetUnitAsString()}";

        public override string ToString() => ToString(null, null);

        public string GetUnitAsString()
        {
            switch (Unit)
            {
                case VolumeUnit.CM3:
                    return "cm³";
                case VolumeUnit.PERCENT:
                    return "%";
                default:
                    return "???";
            }
        }

        public string ValueAsString
        {
            get
            {
                if (double.IsNaN(Value))
                    return "N/A";

                int decimals = _core.DecimalDigits;
                return $"{Value.ToString($"F{decimals}")} {GetUnitAsString()}";
            }
        }


        public static VolumeValue operator +(VolumeValue a, VolumeValue b)
        {
            EnsureSameUnit(a, b);
            return new VolumeValue(a.Value + b.Value, a.Unit);
        }

        public static VolumeValue operator -(VolumeValue a, VolumeValue b)
        {
            EnsureSameUnit(a, b);
            return new VolumeValue(a.Value - b.Value, a.Unit);
        }

        public static VolumeValue operator *(VolumeValue a, double scalar)
        {
            return new VolumeValue(a.Value * scalar, a.Unit);
        }

        public static VolumeValue operator *(double scalar, VolumeValue a)
        {
            return new VolumeValue(a.Value * scalar, a.Unit);
        }

        public static VolumeValue operator /(VolumeValue a, double scalar)
        {
            return new VolumeValue(a.Value / scalar, a.Unit);
        }

        public static bool operator ==(VolumeValue left, VolumeValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VolumeValue left, VolumeValue right)
        {
            return !left.Equals(right);
        }

        public static bool operator >(VolumeValue left, VolumeValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value > right.Value;
        }

        public static bool operator <(VolumeValue left, VolumeValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value < right.Value;
        }

        public static bool operator >=(VolumeValue left, VolumeValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value >= right.Value;
        }

        public static bool operator <=(VolumeValue left, VolumeValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value <= right.Value;
        }


        /// <summary>
        /// Returns the scalar ratio between two volumes (unitless).
        /// </summary>
        public static double operator /(VolumeValue a, VolumeValue b)
        {
            EnsureSameUnit(a, b);
            return a.Value / b.Value;
        }


        private static void EnsureSameUnit(VolumeValue a, VolumeValue b)
        {
            if (a.Unit != b.Unit)
                throw new InvalidOperationException("Incompatible units.");
        }

        public static VolumeValue InPercent(double volume)
        {
            return new VolumeValue(volume, VolumeUnit.PERCENT);
        }

        public static VolumeValue InCM3(double volume)
        {
            return new VolumeValue(volume, VolumeUnit.CM3);
        }
    }
}