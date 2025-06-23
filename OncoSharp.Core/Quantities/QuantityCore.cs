// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace OncoSharp.Core.Quantities
{
    public interface IQuantityCore<TUnit> where TUnit : Enum
    {
        double Value { get; }
        TUnit Unit { get; }
        int DecimalDigits { get; }
        double Error { get; }
    }

    public readonly struct QuantityCore<TUnit> :
        IEquatable<QuantityCore<TUnit>>, IQuantityCore<TUnit> where TUnit : Enum
    {
        public double Value { get; }
        public TUnit Unit { get; }
        public int DecimalDigits { get; }
        public double Error { get; }

        public QuantityCore(double value, TUnit unit, int decimalDigits, double error)
        {
            Value = value;
            Unit = unit;
            DecimalDigits = decimalDigits;
            Error = error;
        }

        public string Format(string format = null, IFormatProvider provider = null)
        {
            if (double.IsNaN(Value)) return "N/A";
            var fmt = format ?? $"F{DecimalDigits}";
            return Value.ToString(fmt, provider ?? CultureInfo.InvariantCulture);
        }

        public bool Equals(QuantityCore<TUnit> other)
        {
            if (double.IsNaN(Value) && double.IsNaN(other.Value))
                return EqualityComparer<TUnit>.Default.Equals(Unit, other.Unit);

            return EqualityComparer<TUnit>.Default.Equals(Unit, other.Unit) &&
                   Math.Abs(Value - other.Value) < Error;
        }

        public int CompareTo(QuantityCore<TUnit> other) => Value.CompareTo(other.Value);

        public override bool Equals(object obj) =>
            obj is QuantityCore<TUnit> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Value.GetHashCode();
                hash = hash * 31 + Unit.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(QuantityCore<TUnit> left, QuantityCore<TUnit> right) =>
            left.Equals(right);

        public static bool operator !=(QuantityCore<TUnit> left, QuantityCore<TUnit> right) =>
            !left.Equals(right);
    }
}