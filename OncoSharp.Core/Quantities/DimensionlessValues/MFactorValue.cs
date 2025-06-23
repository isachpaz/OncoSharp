// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.DimensionlessValues
{
    public struct MFactorValue : IComparable<MFactorValue>, IEquatable<MFactorValue>,
        IQuantityCreation<MFactorValue, UnitLess>
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public MFactorValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            config = config ?? MFactorConfig.Default();

            _core = new QuantityCore<UnitLess>(value, default,
                config.Decimals(default),
                config.Error());
        }


        public string ValueAsString => this.GetValueAsString();

        public static MFactorValue operator -(MFactorValue dv1, MFactorValue dv2)
        {
            return new MFactorValue(dv1.Value - dv2.Value);
        }

        public static MFactorValue operator +(MFactorValue dv1, MFactorValue dv2)
        {
            return new MFactorValue(dv1.Value + dv2.Value);
        }

        public static MFactorValue operator *(MFactorValue dv, double dbl)
        {
            return new MFactorValue(dv.Value * dbl);
        }

        public static MFactorValue operator *(double dbl, MFactorValue dv)
        {
            return new MFactorValue(dv.Value * dbl);
        }

        public static MFactorValue operator /(MFactorValue dv, double dbl)
        {
            return new MFactorValue(dv.Value / dbl);
        }

        public static double operator /(MFactorValue dv1, MFactorValue dv2)
        {
            return dv1.Value / dv2.Value;
        }

        public static MFactorValue operator ^(MFactorValue value, MFactorValue exponent)
        {
            return MFactorValue.New(Math.Pow(value.Value, exponent.Value));
        }

        public static MFactorValue operator ^(MFactorValue value, double exponent)
        {
            return MFactorValue.New(Math.Pow(value.Value, exponent));
        }

        public static MFactorValue operator ^(double value, MFactorValue exponent)
        {
            return MFactorValue.New(Math.Pow(value, exponent.Value));
        }

        public static MFactorValue operator ^(MFactorValue value, int exponent)
        {
            return MFactorValue.New(Math.Pow(value.Value, exponent));
        }

        // Allow implicit conversion
        public static implicit operator MFactorValue(double value)
        {
            return new MFactorValue(value);
        }

        public static implicit operator MFactorValue(int value)
        {
            return new MFactorValue(value);
        }

        public static implicit operator Double(MFactorValue value)
        {
            return value.Value;
        }

        public static bool operator <=(MFactorValue left, MFactorValue right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(MFactorValue left, MFactorValue right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(MFactorValue left, MFactorValue right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(MFactorValue left, MFactorValue right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(MFactorValue left, MFactorValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MFactorValue left, MFactorValue right)
        {
            return !(left == right);
        }


        public int CompareTo(MFactorValue other)
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


        public MFactorValue TNew(double value, UnitLess unit)
        {
            return new MFactorValue(value);
        }

        public override string ToString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.GetValueAsString();
        }


        public static MFactorValue Empty() => new MFactorValue(double.NaN);

        public static MFactorValue Zero => new MFactorValue(0);


        public static MFactorValue New(double value)
        {
            return new MFactorValue(value);
        }

        private string GetValueAsString()
        {
            return double.IsNaN(this.Value) ? "N/A" : this.Value.ToString($"F{_core.DecimalDigits:D}");
        }

        public bool Equals(MFactorValue other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MFactorValue other && Equals(other);
        }

        public override int GetHashCode() => _core.GetHashCode();


        public string GetUnits() => String.Empty;
        public double GetValue() => Value;
    }
}