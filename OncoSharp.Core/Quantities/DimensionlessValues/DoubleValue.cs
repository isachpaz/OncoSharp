// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.DimensionlessValues
{
    public struct DoubleValue : IComparable<DoubleValue>, IEquatable<DoubleValue>,
        IQuantityCreation<DoubleValue, UnitLess>, IQuantityGetters<DoubleValue, UnitLess>
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public DoubleValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            config = config ?? DoubleConfig.Default();

            _core = new QuantityCore<UnitLess>(value, default,
                config.Decimals(default),
                config.Error());
        }

        public string ValueAsString => this.GetValueAsString();

        public static DoubleValue operator -(DoubleValue dv1, DoubleValue dv2)
        {
            return new DoubleValue(dv1.Value - dv2.Value);
        }

        public static DoubleValue operator +(DoubleValue dv1, DoubleValue dv2)
        {
            return new DoubleValue(dv1.Value + dv2.Value);
        }

        public static DoubleValue operator *(DoubleValue dv, double dbl)
        {
            return new DoubleValue(dv.Value * dbl);
        }

        public static DoubleValue operator *(double dbl, DoubleValue dv)
        {
            return new DoubleValue(dv.Value * dbl);
        }

        public static DoubleValue operator /(DoubleValue dv, double dbl)
        {
            return new DoubleValue(dv.Value / dbl);
        }

        public static double operator /(DoubleValue dv1, DoubleValue dv2)
        {
            return dv1.Value / dv2.Value;
        }

        public static DoubleValue operator ^(DoubleValue value, DoubleValue exponent)
        {
            return DoubleValue.New(Math.Pow(value.Value, exponent.Value));
        }

        public static DoubleValue operator ^(DoubleValue value, double exponent)
        {
            return DoubleValue.New(Math.Pow(value.Value, exponent));
        }

        public static DoubleValue operator ^(double value, DoubleValue exponent)
        {
            return DoubleValue.New(Math.Pow(value, exponent.Value));
        }

        public static DoubleValue operator ^(DoubleValue value, int exponent)
        {
            return DoubleValue.New(Math.Pow(value.Value, exponent));
        }

        // Allow implicit conversion
        public static implicit operator DoubleValue(double value)
        {
            return new DoubleValue(value);
        }

        public static implicit operator DoubleValue(int value)
        {
            return new DoubleValue(value);
        }

        public static implicit operator Double(DoubleValue value)
        {
            return value.Value;
        }

        public static bool operator <=(DoubleValue left, DoubleValue right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(DoubleValue left, DoubleValue right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(DoubleValue left, DoubleValue right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(DoubleValue left, DoubleValue right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(DoubleValue left, DoubleValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DoubleValue left, DoubleValue right)
        {
            return !(left == right);
        }


        public int CompareTo(DoubleValue other)
        {
            if (this.Value > other.Value)
            {
                return 1;
            }
            else if (this.Value < other.Value)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }


        public DoubleValue TNew(double value, UnitLess unit)
        {
            return new DoubleValue(value);
        }


        public override string ToString()
        {
            return double.IsNaN(this.Value) ? "N/A" : this.GetValueAsString();
        }


        public static DoubleValue Empty()
        {
            return new DoubleValue(double.NaN);
        }

        public static DoubleValue Zero => new DoubleValue(0);


        public static DoubleValue New(double value)
        {
            return new DoubleValue(value);
        }

        private string GetValueAsString()
        {
            return double.IsNaN(this.Value) ? "N/A" : this.Value.ToString($"F{_core.DecimalDigits:D}");
        }

        public bool Equals(DoubleValue other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DoubleValue other && Equals(other);
        }

        public override int GetHashCode() => _core.GetHashCode();

        public double GetValue() => Value;

        UnitLess IQuantityGetters<DoubleValue, UnitLess>.GetUnits() => _core.Unit;
    }
}