// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.DimensionlessValues;

namespace OncoSharp.Core.Quantities.Probability
{
    public readonly struct ProbabilityValue :
        IEquatable<ProbabilityValue>,
        IComparable<ProbabilityValue>,
        IFormattable
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public ProbabilityValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            if (value < 0.0 || value > 1.0)
                throw new ArgumentOutOfRangeException(nameof(value), "Probability must be between 0 and 1.");

            config = config ?? ProbabilityConfig.Default();

            _core = new QuantityCore<UnitLess>(value, default,
                config.Decimals(default),
                config.Error());
        }

        public static ProbabilityValue New(double value, IQuantityConfig<UnitLess> config = null) =>
            new ProbabilityValue(value, config);

        public static ProbabilityValue Zero => new ProbabilityValue(0.0);
        public static ProbabilityValue One => new ProbabilityValue(1.0);
        public static ProbabilityValue Empty() => new ProbabilityValue(double.NaN);

        public bool IsEmpty => this == Empty();

        public int CompareTo(ProbabilityValue other) => _core.CompareTo(other._core);
        public bool Equals(ProbabilityValue other) => _core.Equals(other._core);

        public override bool Equals(object obj) => obj is ProbabilityValue other && Equals(other);
        public override int GetHashCode() => _core.GetHashCode();

        public override string ToString() => ToString(null, null);

        public string ToString(string format, IFormatProvider formatProvider) =>
            _core.Format(format, formatProvider);

        public string ValueAsString
        {
            get
            {
                if (double.IsNaN(Value))
                    return "N/A";

                int decimals = _core.DecimalDigits;
                return Value.ToString($"F{decimals}");
            }
        }

        public static ProbabilityValue operator +(ProbabilityValue a, ProbabilityValue b)
        {
            return new ProbabilityValue(a.Value + b.Value);
        }

        public static ProbabilityValue operator -(ProbabilityValue a, ProbabilityValue b)
        {
            return new ProbabilityValue(a.Value - b.Value);
        }

        public static ProbabilityValue operator *(ProbabilityValue a, double scalar)
        {
            return new ProbabilityValue(a.Value * scalar);
        }

        public static ProbabilityValue operator *(double scalar, ProbabilityValue a)
        {
            return new ProbabilityValue(a.Value * scalar);
        }

        public static ProbabilityValue operator *(ProbabilityValue a, ProbabilityValue b)
        {
            return new ProbabilityValue(a.Value * b.Value);
        }

        public static ProbabilityValue operator /(ProbabilityValue a, double scalar)
        {
            return new ProbabilityValue(a.Value / scalar);
        }

        public static double operator /(ProbabilityValue a, ProbabilityValue b)
        {
            return a.Value / b.Value;
        }

        public static bool operator ==(ProbabilityValue left, ProbabilityValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProbabilityValue left, ProbabilityValue right)
        {
            return !left.Equals(right);
        }

        public static bool operator >(ProbabilityValue left, ProbabilityValue right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <(ProbabilityValue left, ProbabilityValue right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >=(ProbabilityValue left, ProbabilityValue right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <=(ProbabilityValue left, ProbabilityValue right)
        {
            return left.Value <= right.Value;
        }
    }
}