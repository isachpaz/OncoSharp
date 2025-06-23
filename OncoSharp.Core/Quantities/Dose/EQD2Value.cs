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
    public struct EQD2Value :
        IComparable<EQD2Value>,
        IEquatable<EQD2Value>,
        IQuantityCreation<EQD2Value, DoseUnit>,
        IQuantityGetters<EQD2Value, DoseUnit>,
        IQuantityArithmetic<EQD2Value>
    {
        private readonly QuantityCore<DoseUnit> _core;

        public double Value => _core.Value;
        public DoseUnit Unit => _core.Unit;

        public EQD2Value(double value, DoseUnit unit, IQuantityConfig<DoseUnit> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));

            config = config ?? DoseConfig.Default();

            _core = new QuantityCore<DoseUnit>(checkedValue, unit,
                config.Decimals(unit),
                config.Error());
        }

        public string UnitAsString => this.GetUnitAsString();
        public string ValueAsString => this.GetValueAsString();

        public static EQD2Value operator -(EQD2Value dv1, EQD2Value dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return new EQD2Value(dv1.Value - dv2.Value, dv1.Unit);
        }

        public static EQD2Value operator +(EQD2Value dv1, EQD2Value dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return new EQD2Value(dv1.Value + dv2.Value, dv1.Unit);
        }

        public static EQD2Value operator *(EQD2Value dv, double dbl)
        {
            return new EQD2Value(dv.Value * dbl, dv.Unit);
        }

        public static EQD2Value operator *(double dbl, EQD2Value dv)
        {
            return new EQD2Value(dv.Value * dbl, dv.Unit);
        }

        public static EQD2Value operator /(EQD2Value dv, double dbl)
        {
            return new EQD2Value(dv.Value / dbl, dv.Unit);
        }

        public static double operator /(EQD2Value dv1, EQD2Value dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return dv1.Value / dv2.Value;
        }

        public static EQD2Value operator ^(EQD2Value value, EQD2Value exponent)
        {
            if (value.Unit != exponent.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return EQD2Value.New(Math.Pow(value.Value, exponent.Value), value.Unit);
        }

        public static EQD2Value operator ^(EQD2Value value, double exponent)
        {
            return EQD2Value.New(Math.Pow(value.Value, exponent), value.Unit);
        }

        public static EQD2Value operator ^(double value, EQD2Value exponent)
        {
            return EQD2Value.New(Math.Pow(value, exponent.Value), exponent.Unit);
        }

        public static EQD2Value operator ^(EQD2Value value, int exponent)
        {
            return EQD2Value.New(Math.Pow(value.Value, exponent), value.Unit);
        }

        public static bool operator <=(EQD2Value left, EQD2Value right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(EQD2Value left, EQD2Value right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(EQD2Value left, EQD2Value right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(EQD2Value left, EQD2Value right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(EQD2Value left, EQD2Value right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EQD2Value left, EQD2Value right)
        {
            return !(left == right);
        }


        public int CompareTo(EQD2Value other)
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

        public EQD2Value Add(EQD2Value valueT)
        {
            if (valueT.Unit != this.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return New(valueT.Value + this.Value, valueT.Unit);
        }

        public EQD2Value Subtract(EQD2Value valueT)
        {
            if (valueT.Unit != this.Unit)
                throw new ArithmeticException("Dose units cannot be different.");
            return New(this.Value - valueT.Value, this.Unit);
        }

        public EQD2Value Multiply(double dValue)
        {
            return New(this.Value * dValue, this.Unit);
        }

        public EQD2Value TNew(double value, DoseUnit unit)
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

        public static EQD2Value Empty() => new EQD2Value(double.NaN, DoseUnit.UNKNOWN);
        public static EQD2Value ZeroInGy() => new EQD2Value(0, DoseUnit.Gy);

        public static EQD2Value InGy(double value, IQuantityConfig<DoseUnit> config = null)
        {
            CheckDoseLimits(value);
            return new EQD2Value(value, DoseUnit.Gy, config);
        }

        public static EQD2Value InGy(double? value, IQuantityConfig<DoseUnit> config = null)
        {
            if (value == null) return ZeroInGy();
            return InGy(value.Value, config);
        }

        public static EQD2Value InCGy(double value, IQuantityConfig<DoseUnit> config = null)
        {
            CheckDoseLimits(value);
            return new EQD2Value(value, DoseUnit.cGy, config);
        }

        public static EQD2Value New(double value, DoseUnit unit, IQuantityConfig<DoseUnit> config = null)
        {
            return new EQD2Value(value, unit, config);
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
            return string.Format($"{{0:F{_core.DecimalDigits}}}", this.Value);
        }

        public bool Equals(EQD2Value other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error &&
                   Unit == other.Unit;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EQD2Value other && Equals(other);
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

        public double GetValue()
        {
            return Value;
        }

        DoseUnit IQuantityGetters<EQD2Value, DoseUnit>.GetUnits()
        {
            throw new NotImplementedException();
        }


        public string GetUnits() => this.UnitAsString;


        public static IEnumerable<EQD2Value> Range(double start, double stop, double step,
            bool bIncludeLastValue,
            DoseUnit unit)
        {
            throw new NotImplementedException();
            //return DoseHelper.EQD2Range(start, stop, step, bIncludeLastValue, unit);
        }
    }
}