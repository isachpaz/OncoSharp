// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.DimensionlessValues
{
    public readonly struct SerialityFactorValue : IComparable<SerialityFactorValue>, IEquatable<SerialityFactorValue>,
        IQuantityCreation<SerialityFactorValue, UnitLess>
    {
        public override bool Equals(object obj)
        {
            return obj is SerialityFactorValue other && Equals(other);
        }

        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public SerialityFactorValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            config = config ?? SerialityFactorConfig.Default();

            _core = new QuantityCore<UnitLess>(value, default,
                config.Decimals(default),
                config.Error());
        }


        public string ValueAsString => this.GetValueAsString();

        public static SerialityFactorValue operator -(SerialityFactorValue dv1, SerialityFactorValue dv2)
        {
            return new SerialityFactorValue(dv1.Value - dv2.Value);
        }

        public static SerialityFactorValue operator +(SerialityFactorValue dv1, SerialityFactorValue dv2)
        {
            return new SerialityFactorValue(dv1.Value + dv2.Value);
        }

        public static SerialityFactorValue operator *(SerialityFactorValue dv, double dbl)
        {
            return new SerialityFactorValue(dv.Value * dbl);
        }

        public static SerialityFactorValue operator *(double dbl, SerialityFactorValue dv)
        {
            return new SerialityFactorValue(dv.Value * dbl);
        }

        public static SerialityFactorValue operator /(SerialityFactorValue dv, double dbl)
        {
            return new SerialityFactorValue(dv.Value / dbl);
        }

        public static double operator /(SerialityFactorValue dv1, SerialityFactorValue dv2)
        {
            return dv1.Value / dv2.Value;
        }

        public static SerialityFactorValue operator ^(SerialityFactorValue value, SerialityFactorValue exponent)
        {
            return SerialityFactorValue.New(Math.Pow(value.Value, exponent.Value));
        }

        public static SerialityFactorValue operator ^(SerialityFactorValue value, double exponent)
        {
            return SerialityFactorValue.New(Math.Pow(value.Value, exponent));
        }

        public static SerialityFactorValue operator ^(double value, SerialityFactorValue exponent)
        {
            return SerialityFactorValue.New(Math.Pow(value, exponent.Value));
        }

        public static SerialityFactorValue operator ^(SerialityFactorValue value, int exponent)
        {
            return SerialityFactorValue.New(Math.Pow(value.Value, exponent));
        }

        // Allow implicit conversion
        public static implicit operator SerialityFactorValue(double value)
        {
            return new SerialityFactorValue(value);
        }

        public static implicit operator SerialityFactorValue(int value)
        {
            return new SerialityFactorValue(value);
        }

        public static implicit operator Double(SerialityFactorValue value)
        {
            return value.Value;
        }

        public static bool operator <=(SerialityFactorValue left, SerialityFactorValue right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(SerialityFactorValue left, SerialityFactorValue right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(SerialityFactorValue left, SerialityFactorValue right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(SerialityFactorValue left, SerialityFactorValue right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(SerialityFactorValue left, SerialityFactorValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SerialityFactorValue left, SerialityFactorValue right)
        {
            return !(left == right);
        }


        public SerialityFactorValue TNew(double value, UnitLess unit) => new SerialityFactorValue(value);
        public int CompareTo(SerialityFactorValue other) => _core.CompareTo(other._core);
        public bool Equals(SerialityFactorValue other) => _core.Equals(other._core);

        public override string ToString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.GetValueAsString();
        }


        public static SerialityFactorValue Empty => new SerialityFactorValue(double.NaN);

        public static SerialityFactorValue Zero => new SerialityFactorValue(0);


        public static SerialityFactorValue New(double value) => new SerialityFactorValue(value);

        private string GetValueAsString()
        {
            return double.IsNaN(this.Value) ? "N/A" : this.Value.ToString($"F{_core.DecimalDigits:D}");
        }


        public override int GetHashCode() => _core.GetHashCode();
        public double GetValue() => Value;
        public string GetUnits() => String.Empty;

        public SerialityFactorValue TNew(double value, string unit) => new SerialityFactorValue(value);
    }
}