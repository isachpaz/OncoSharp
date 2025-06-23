// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.DimensionlessValues
{
    public struct GammaValue : IComparable<GammaValue>, IEquatable<GammaValue>,
        IQuantityCreation<GammaValue, UnitLess>, IQuantityGetters<GammaValue, UnitLess>
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public GammaValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            config = config ?? GammaConfig.Default();

            _core = new QuantityCore<UnitLess>(value, default,
                config.Decimals(default),
                config.Error());
        }


        public string ValueAsString => this.GetValueAsString();

        public static GammaValue operator -(GammaValue dv1, GammaValue dv2)
        {
            return new GammaValue(dv1.Value - dv2.Value);
        }

        public static GammaValue operator +(GammaValue dv1, GammaValue dv2)
        {
            return new GammaValue(dv1.Value + dv2.Value);
        }

        public static GammaValue operator *(GammaValue dv, double dbl)
        {
            return new GammaValue(dv.Value * dbl);
        }

        public static GammaValue operator *(double dbl, GammaValue dv)
        {
            return new GammaValue(dv.Value * dbl);
        }

        public static GammaValue operator /(GammaValue dv, double dbl)
        {
            return new GammaValue(dv.Value / dbl);
        }

        public static GammaValue operator /(GammaValue dv, int dbl)
        {
            return new GammaValue(dv.Value / dbl);
        }

        public static double operator /(GammaValue dv1, GammaValue dv2)
        {
            return dv1.Value / dv2.Value;
        }

        public static GammaValue operator ^(GammaValue value, GammaValue exponent)
        {
            return GammaValue.New(Math.Pow(value.Value, exponent.Value));
        }

        public static GammaValue operator ^(GammaValue value, double exponent)
        {
            return GammaValue.New(Math.Pow(value.Value, exponent));
        }

        public static GammaValue operator ^(double value, GammaValue exponent)
        {
            return GammaValue.New(Math.Pow(value, exponent.Value));
        }

        public static GammaValue operator ^(GammaValue value, int exponent)
        {
            return GammaValue.New(Math.Pow(value.Value, exponent));
        }

        // Allow implicit conversion
        public static implicit operator GammaValue(double value)
        {
            return new GammaValue(value);
        }

        public static implicit operator GammaValue(int value)
        {
            return new GammaValue(value);
        }

        public static implicit operator Double(GammaValue value)
        {
            return value.Value;
        }

        public static bool operator <=(GammaValue left, GammaValue right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(GammaValue left, GammaValue right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(GammaValue left, GammaValue right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(GammaValue left, GammaValue right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(GammaValue left, GammaValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GammaValue left, GammaValue right)
        {
            return !(left == right);
        }


        public int CompareTo(GammaValue other)
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


        public GammaValue TNew(double value, UnitLess unit)
        {
            return new GammaValue(value);
        }

        public override string ToString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.GetValueAsString();
        }


        public static GammaValue Empty()
        {
            return new GammaValue(double.NaN);
        }

        public static GammaValue Zero => new GammaValue(0);

        public static GammaValue New(double value)
        {
            return new GammaValue(value);
        }

        private string GetValueAsString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.Value.ToString($"F{_core.DecimalDigits:D}");
        }

        public bool Equals(GammaValue other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GammaValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ValueAsString.GetHashCode();
                return hashCode;
            }
        }

        public double GetValue()
        {
            return Value;
        }

        UnitLess IQuantityGetters<GammaValue, UnitLess>.GetUnits() => _core.Unit;

        public string GetUnits() => String.Empty;
    }
}