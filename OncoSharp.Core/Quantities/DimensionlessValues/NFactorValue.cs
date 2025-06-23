// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.DimensionlessValues
{
    public struct NFactorValue : IComparable<NFactorValue>, IEquatable<NFactorValue>,
        IQuantityCreation<NFactorValue, UnitLess>
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public NFactorValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            config = config ?? NFactorConfig.Default();

            _core = new QuantityCore<UnitLess>(value, default,
                config.Decimals(default),
                config.Error());
        }


        public double GetAlphaVolumeEffect() => 1.0 / Value;

        public string ValueAsString => this.GetValueAsString();

        public static NFactorValue operator -(NFactorValue dv1, NFactorValue dv2)
        {
            return new NFactorValue(dv1.Value - dv2.Value);
        }

        public static NFactorValue operator +(NFactorValue dv1, NFactorValue dv2)
        {
            return new NFactorValue(dv1.Value + dv2.Value);
        }

        public static NFactorValue operator *(NFactorValue dv, double dbl)
        {
            return new NFactorValue(dv.Value * dbl);
        }

        public static NFactorValue operator *(double dbl, NFactorValue dv)
        {
            return new NFactorValue(dv.Value * dbl);
        }

        public static NFactorValue operator /(NFactorValue dv, double dbl)
        {
            return new NFactorValue(dv.Value / dbl);
        }

        public static double operator /(NFactorValue dv1, NFactorValue dv2)
        {
            return dv1.Value / dv2.Value;
        }

        public static NFactorValue operator ^(NFactorValue value, NFactorValue exponent)
        {
            return NFactorValue.New(Math.Pow(value.Value, exponent.Value));
        }

        public static NFactorValue operator ^(NFactorValue value, double exponent)
        {
            return NFactorValue.New(Math.Pow(value.Value, exponent));
        }

        public static NFactorValue operator ^(double value, NFactorValue exponent)
        {
            return NFactorValue.New(Math.Pow(value, exponent.Value));
        }

        public static NFactorValue operator ^(NFactorValue value, int exponent)
        {
            return NFactorValue.New(Math.Pow(value.Value, exponent));
        }

        // Allow implicit conversion
        public static implicit operator NFactorValue(double value)
        {
            return new NFactorValue(value);
        }

        public static implicit operator NFactorValue(int value)
        {
            return new NFactorValue(value);
        }

        public static implicit operator Double(NFactorValue value)
        {
            return value.Value;
        }

        public static bool operator <=(NFactorValue left, NFactorValue right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(NFactorValue left, NFactorValue right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(NFactorValue left, NFactorValue right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(NFactorValue left, NFactorValue right)
        {
            return left.Value > right.Value;
        }

        public NFactorValue TNew(double value, UnitLess unit) => new NFactorValue(value);

        public int CompareTo(NFactorValue other) => _core.CompareTo(other._core);

        public override string ToString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.GetValueAsString();
        }


        public static NFactorValue Empty() => new NFactorValue(double.NaN);
        public static NFactorValue Zero => new NFactorValue(0);


        public static NFactorValue New(double value) => new NFactorValue(value);

        private string GetValueAsString()
        {
            return double.IsNaN(this.Value) ? "N/A" : this.Value.ToString($"F{_core.DecimalDigits:D}");
        }


        public override bool Equals(object obj)
        {
            return obj is NFactorValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _core.GetHashCode();
        }

        public double GetValue() => Value;
        public string GetUnits() => String.Empty;

        public bool Equals(NFactorValue other)
        {
            return _core.Equals(other._core);
        }
    }
}