// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.Core.Quantities.PartialOxygen
{
    public struct PO2Value : IEquatable<PO2Value>,
        IComparable<PO2Value>,
        IFormattable,
        IQuantityCreation<PO2Value, PO2Unit>,
        IQuantityArithmetic<PO2Value>,
        IQuantityGetters<PO2Value, PO2Unit>
    {
        public static IQuantityConfig<PO2Unit> Config { get; set; } = new DefaultPO2Config();

        private readonly QuantityCore<PO2Unit> _core;
        public double Value => _core.Value;
        public PO2Unit Unit => _core.Unit;

        public PO2Value(double value, PO2Unit unit, IQuantityConfig<VolumeUnit> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));

            config = config ?? VolumeConfig.Default();

            _core = new QuantityCore<PO2Unit>(checkedValue, unit, Config.Decimals(unit), Config.Error());
        }

        public static PO2Value FromUnit(double value, PO2Unit unit) => new PO2Value(value, unit);
        public static PO2Value Empty() => new PO2Value(double.NaN, PO2Unit.UNKNOWN);
        public static PO2Value ZeroInmmHg => new PO2Value(0, PO2Unit.mmHg);

        public static PO2Value New(double value, PO2Unit unit) => new PO2Value(value, unit);

        public static PO2Value InmmHg(double value)
        {
            return new PO2Value(value, PO2Unit.mmHg);
        }


        public int CompareTo(PO2Value other) => _core.CompareTo(other._core);

        public bool Equals(PO2Value other) => _core.Equals(other._core);

        public override bool Equals(object obj) => obj is PO2Value other && Equals(other);

        public override int GetHashCode() => _core.GetHashCode();

        public string ToString(string format, IFormatProvider formatProvider) =>
            $"{_core.Format(format, formatProvider)} {GetUnitAsString()}";

        public override string ToString()
        {
            if (double.IsNaN(Value))
                return "N/A";
            return $"{GetValueAsString()} {GetUnitAsString()}";
        }

        private string GetUnitAsString()
        {
            switch (Unit)
            {
                case PO2Unit.mmHg:
                    return "mmHg";
                default:
                    return "???";
            }
        }

        private string GetValueAsString()
        {
            if (double.IsNaN(Value))
                return "N/A";
            return Value.ToString($"F{Config.Decimals(Unit)}");
        }

        public string UnitAsString => GetUnitAsString();
        public string ValueAsString => GetValueAsString();

        public double GetValue() => Value;

        PO2Unit IQuantityGetters<PO2Value, PO2Unit>.GetUnits() => Unit;

        public PO2Value TNew(double value, PO2Unit unit) => new PO2Value(value, unit);

        public PO2Value Add(PO2Value valueT)
        {
            if (valueT.Unit != Unit)
                throw new ArithmeticException("PO2 units cannot be different.");
            return New(Value + valueT.Value, Unit);
        }

        public PO2Value Subtract(PO2Value valueT)
        {
            if (valueT.Unit != Unit)
                throw new ArithmeticException("PO2 units cannot be different.");
            return New(Value - valueT.Value, Unit);
        }

        public PO2Value Multiply(double dValue) => New(Value * dValue, Unit);

        // Operators
        public static PO2Value operator +(PO2Value a, PO2Value b) => a.Add(b);
        public static PO2Value operator -(PO2Value a, PO2Value b) => a.Subtract(b);
        public static PO2Value operator *(PO2Value a, double b) => a.Multiply(b);
        public static PO2Value operator *(double b, PO2Value a) => a.Multiply(b);
        public static PO2Value operator /(PO2Value a, double b) => New(a.Value / b, a.Unit);
        public static PO2Value operator /(PO2Value a, int b) => New(a.Value / b, a.Unit);

        public static double operator /(PO2Value a, PO2Value b)
        {
            if (a.Unit != b.Unit)
                throw new ArithmeticException("PO2 units must match.");
            return a.Value / b.Value;
        }

        public static double operator ^(PO2Value baseVal, double exp) =>
            New(Math.Pow(baseVal.Value, exp), baseVal.Unit);

        public static double operator ^(PO2Value baseVal, int exp) =>
            New(Math.Pow(baseVal.Value, exp), baseVal.Unit);

        public static double operator ^(double baseVal, PO2Value expVal) =>
            New(Math.Pow(baseVal, expVal.Value), expVal.Unit);

        // Comparison operators
        public static bool operator ==(PO2Value left, PO2Value right) => left.Equals(right);
        public static bool operator !=(PO2Value left, PO2Value right) => !(left == right);
        public static bool operator <(PO2Value left, PO2Value right) => left.CompareTo(right) < 0;
        public static bool operator >(PO2Value left, PO2Value right) => left.CompareTo(right) > 0;
        public static bool operator <=(PO2Value left, PO2Value right) => left.CompareTo(right) <= 0;
        public static bool operator >=(PO2Value left, PO2Value right) => left.CompareTo(right) >= 0;

        public static implicit operator PO2Value(double value) => new PO2Value(value, PO2Unit.mmHg);
        public static implicit operator double(PO2Value value) => value.Value;
    }
}