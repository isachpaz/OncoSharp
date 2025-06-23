// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.Dose
{
    public readonly struct DoseValue :
        IEquatable<DoseValue>,
        IComparable<DoseValue>,
        IFormattable,
        IQuantityCreation<DoseValue, DoseUnit>,
        IQuantityGetters<DoseValue, DoseUnit>,
        IQuantityArithmetic<DoseValue>
    {
        private readonly QuantityCore<DoseUnit> _core;

        public double Value => _core.Value;
        public DoseUnit Unit => _core.Unit;

        public DoseValue(double value, DoseUnit unit, IQuantityConfig<DoseUnit> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));

            config = config ?? DoseConfig.Default();

            _core = new QuantityCore<DoseUnit>(checkedValue, unit,
                config.Decimals(unit),
                config.Error());
        }

        public static DoseValue New(double value, DoseUnit unit) => new DoseValue(value, unit);
        public static DoseValue Empty() => new DoseValue(double.NaN, DoseUnit.UNKNOWN);

        public int CompareTo(DoseValue other) => _core.CompareTo(other._core);
        public bool Equals(DoseValue other) => _core.Equals(other._core);

        public override bool Equals(object obj) => obj is DoseValue other && Equals(other);
        public override int GetHashCode() => _core.GetHashCode();

        public string ToString(string format, IFormatProvider formatProvider) =>
            $"{_core.Format(format, formatProvider)} {GetUnitAsString()}";

        public DoseValue TNew(double value, DoseUnit unit)
        {
            return new DoseValue(value, unit);
        }


        public override string ToString() => ToString(null, null);

        public string GetUnitAsString()
        {
            switch (Unit)
            {
                case DoseUnit.Gy:
                    return "Gy";
                case DoseUnit.cGy:
                    return "cGy";
                case DoseUnit.PERCENT:
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

        public static DoseValue operator +(DoseValue a, DoseValue b)
        {
            EnsureSameUnit(a, b);
            return new DoseValue(a.Value + b.Value, a.Unit);
        }

        public static DoseValue operator -(DoseValue a, DoseValue b)
        {
            EnsureSameUnit(a, b);
            return new DoseValue(a.Value - b.Value, a.Unit);
        }

        public static DoseValue operator *(DoseValue a, double scalar)
        {
            return new DoseValue(a.Value * scalar, a.Unit);
        }

        public static DoseValue operator *(double scalar, DoseValue a)
        {
            return new DoseValue(a.Value * scalar, a.Unit);
        }

        public static DoseValue operator /(DoseValue a, double scalar)
        {
            return new DoseValue(a.Value / scalar, a.Unit);
        }

        public static double operator /(DoseValue a, DoseValue b)
        {
            EnsureSameUnit(a, b);
            return a.Value / b.Value;
        }

        public static bool operator ==(DoseValue left, DoseValue right) => left.Equals(right);
        public static bool operator !=(DoseValue left, DoseValue right) => !left.Equals(right);

        public static bool operator >(DoseValue left, DoseValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value > right.Value;
        }

        public static bool operator <(DoseValue left, DoseValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value < right.Value;
        }

        public static bool operator >=(DoseValue left, DoseValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value >= right.Value;
        }

        public static bool operator <=(DoseValue left, DoseValue right)
        {
            EnsureSameUnit(left, right);
            return left.Value <= right.Value;
        }

        private static void EnsureSameUnit(DoseValue a, DoseValue b)
        {
            if (a.Unit != b.Unit)
                throw new InvalidOperationException("Incompatible dose units.");
        }

        public static DoseValue InGy(double dose) => new DoseValue(dose, DoseUnit.Gy);
        public static DoseValue InCGy(double dose) => new DoseValue(dose, DoseUnit.cGy);
        public static DoseValue InPercent(double dose) => new DoseValue(dose, DoseUnit.PERCENT);

        public double GetValue() => Value;
        public DoseUnit GetUnits() => Unit;

        public DoseValue Add(DoseValue valueT)
        {
            if (valueT.Unit != this.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return New(valueT.Value + this.Value, valueT.Unit);
        }

        public DoseValue Subtract(DoseValue valueT)
        {
            if (valueT.Unit != this.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return New(this.Value - valueT.Value, this.Unit);
        }

        public DoseValue Multiply(double dValue)
        {
            return New(this.Value * dValue, this.Unit);
        }
    }
}