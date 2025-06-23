// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.Dose
{
    public struct EQD0Value :
        IComparable<EQD0Value>,
        IEquatable<EQD0Value>,
        IQuantityCreation<EQD0Value, DoseUnit>,
        IQuantityArithmetic<EQD0Value>,
        IQuantityGetters<EQD0Value, DoseUnit>
    {
        private readonly QuantityCore<DoseUnit> _core;

        public double Value => _core.Value;
        public DoseUnit Unit => _core.Unit;

        public EQD0Value(double value, DoseUnit unit, IQuantityConfig<DoseUnit> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));

            config = config ?? DoseConfig.Default();

            _core = new QuantityCore<DoseUnit>(checkedValue, unit,
                config.Decimals(unit),
                config.Error());
        }


        public string UnitAsString => this.GetUnitAsString();
        public string ValueAsString => this.GetValueAsString();

        public static EQD0Value operator -(EQD0Value dv1, EQD0Value dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return new EQD0Value(dv1.Value - dv2.Value, dv1.Unit);
        }

        public static EQD0Value operator +(EQD0Value dv1, EQD0Value dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return new EQD0Value(dv1.Value + dv2.Value, dv1.Unit);
        }

        public static EQD0Value operator *(EQD0Value dv, double dbl)
        {
            return new EQD0Value(dv.Value * dbl, dv.Unit);
        }

        public static EQD0Value operator *(double dbl, EQD0Value dv)
        {
            return new EQD0Value(dv.Value * dbl, dv.Unit);
        }

        public static EQD0Value operator /(EQD0Value dv, double dbl)
        {
            return new EQD0Value(dv.Value / dbl, dv.Unit);
        }

        public static double operator /(EQD0Value dv1, EQD0Value dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return dv1.Value / dv2.Value;
        }

        public static EQD0Value operator ^(EQD0Value value, EQD0Value exponent)
        {
            if (value.Unit != exponent.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return EQD0Value.New(Math.Pow(value.Value, exponent.Value), value.Unit);
        }

        public static EQD0Value operator ^(EQD0Value value, double exponent)
        {
            return EQD0Value.New(Math.Pow(value.Value, exponent), value.Unit);
        }

        public static EQD0Value operator ^(double value, EQD0Value exponent)
        {
            return EQD0Value.New(Math.Pow(value, exponent.Value), exponent.Unit);
        }

        public static EQD0Value operator ^(EQD0Value value, int exponent)
        {
            return EQD0Value.New(Math.Pow(value.Value, exponent), value.Unit);
        }

        public static bool operator <=(EQD0Value left, EQD0Value right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(EQD0Value left, EQD0Value right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(EQD0Value left, EQD0Value right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(EQD0Value left, EQD0Value right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(EQD0Value left, EQD0Value right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EQD0Value left, EQD0Value right)
        {
            return !(left == right);
        }


        public int CompareTo(EQD0Value other)
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

        public EQD0Value Add(EQD0Value valueT)
        {
            if (valueT.Unit != this.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return New(valueT.Value + this.Value, valueT.Unit);
        }

        public EQD0Value Subtract(EQD0Value valueT)
        {
            if (valueT.Unit != this.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return New(this.Value - valueT.Value, this.Unit);
        }

        public EQD0Value Multiply(double dValue)
        {
            return New(this.Value * dValue, this.Unit);
        }

        public EQD0Value TNew(double value, DoseUnit unit)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.GetValueAsString() + " " + this.GetUnitAsString();
        }

        private static DoseUnit UnitFrom(string unitName)
        {
            switch (unitName.ToUpper())
            {
                case "GY":
                    return DoseUnit.Gy;
                case "CGY":
                    return DoseUnit.cGy;
                case "%":
                    return DoseUnit.PERCENT;
                default:
                    return DoseUnit.UNKNOWN;
            }
        }

        public static EQD0Value Empty() => new EQD0Value(double.NaN, DoseUnit.UNKNOWN);

        public static EQD0Value ZeroInGy() => new EQD0Value(0, DoseUnit.Gy);

        public static EQD0Value InGy(double value, IQuantityConfig<DoseUnit> config = null)
        {
            CheckDoseLimits(value);
            return new EQD0Value(value, DoseUnit.Gy, config);
        }

        public static EQD0Value InCGy(double value, IQuantityConfig<DoseUnit> config = null)
        {
            CheckDoseLimits(value);
            return new EQD0Value(value, DoseUnit.cGy, config);
        }

        public static EQD0Value New(double value, DoseUnit unit, IQuantityConfig<DoseUnit> config = null)
        {
            return new EQD0Value(value, unit, config);
        }

        private static void CheckDoseLimits(double value)
        {
            if (value < 0.0) throw new ArgumentException("Dose cannot be less than zero.");
        }

        private string GetUnitAsString()
        {
            switch (this.Unit)
            {
                case DoseUnit.Gy:
                    return string.Format("Gy");
                case DoseUnit.cGy:
                    return string.Format("cGy");
                case DoseUnit.PERCENT:
                    return string.Format("%");
                default:
                    return string.Format("???");
            }
        }

        private string GetValueAsString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return string.Format($"{{0:F{_core.DecimalDigits:D}{(object)"}"}", this.Value);
        }

        public bool Equals(EQD0Value other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error &&
                   Unit == other.Unit;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EQD0Value other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Value.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Unit;
                return hashCode;
            }
        }

        public double GetValue() => Value;

        DoseUnit IQuantityGetters<EQD0Value, DoseUnit>.GetUnits()
        {
            throw new NotImplementedException();
        }

        public string GetUnits() => UnitAsString;

        public static IEnumerable<EQD0Value> Range(double start, double stop, double step,
            bool bIncludeLastValue,
            DoseUnit unit)
        {
            throw new NotImplementedException();
            //return DoseHelper.EQD0Range(start, stop, step, bIncludeLastValue, unit);
        }
    }
}