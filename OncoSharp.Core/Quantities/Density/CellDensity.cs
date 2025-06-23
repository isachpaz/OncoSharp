// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;

namespace OncoSharp.Core.Quantities.Density
{
    public readonly struct CellDensity : IComparable<CellDensity>, IEquatable<CellDensity>
    {
        public override bool Equals(object obj)
        {
            return obj is CellDensity other && Equals(other);
        }

        private readonly QuantityCore<CellDensityUnit> _core;

        public double Value => _core.Value;
        public CellDensityUnit Unit => _core.Unit;

        public CellDensity(double value, CellDensityUnit unit, IQuantityConfig<CellDensityUnit> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));

            config = config ?? CellDensityConfig.Default();

            _core = new QuantityCore<CellDensityUnit>(checkedValue, unit,
                config.Decimals(unit),
                config.Error());
        }

        private static CellDensityUnit UnitFrom(string unitName)
        {
            switch (unitName.ToLower())
            {
                case "cells/cm3":
                case "cells/cm³":
                case "cells/cc":
                    return CellDensityUnit.Cells_per_CM3;
                case "cells/mm3":
                case "cells/mm³":
                    return CellDensityUnit.Cells_per_MM3;
                default:
                    return CellDensityUnit.UNKNOWN;
            }
        }


        public string UnitAsString => this.GetUnitAsString();
        public string ValueAsString => this.GetValueAsString();

        public static CellDensity operator -(CellDensity dv1, CellDensity dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Units does not match.");
            return new CellDensity(dv1.Value - dv2.Value, dv1.Unit);
        }

        public static CellDensity operator +(CellDensity dv1, CellDensity dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Units does not match.");
            return new CellDensity(dv1.Value + dv2.Value, dv1.Unit);
        }

        public static CellDensity operator *(CellDensity dv, double dbl)
        {
            return new CellDensity(dv.Value * dbl, dv.Unit);
        }

        public static CellDensity operator *(double dbl, CellDensity dv)
        {
            return new CellDensity(dv.Value * dbl, dv.Unit);
        }

        public static CellDensity operator /(CellDensity dv, double dbl)
        {
            return new CellDensity(dv.Value / dbl, dv.Unit);
        }

        public static double operator /(CellDensity dv1, CellDensity dv2)
        {
            if (dv1.Unit != dv2.Unit)
                throw new ArithmeticException("Units does not match.");
            return dv1.Value / dv2.Value;
        }

        public static bool operator <=(CellDensity left, CellDensity right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(CellDensity left, CellDensity right)
        {
            return left.Value >= right.Value;
        }

        public static bool operator <(CellDensity left, CellDensity right)
        {
            return left.Value < right.Value;
        }

        public static bool operator >(CellDensity left, CellDensity right)
        {
            return left.Value > right.Value;
        }


        public static bool operator ==(CellDensity left, CellDensity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CellDensity left, CellDensity right)
        {
            return !(left == right);
        }


        public bool Equals(CellDensity other) => _core.Equals(other._core);

        public int CompareTo(CellDensity other) => _core.CompareTo(other._core);


        public override string ToString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return this.GetValueAsString() + " " + this.GetUnitAsString();
        }

        public static CellDensity Empty()
        {
            return new CellDensity(double.NaN, CellDensityUnit.UNKNOWN);
        }


        public static CellDensity InCells_Per_CM3(double value, IQuantityConfig<CellDensityUnit> config = null)
        {
            return new CellDensity(value, CellDensityUnit.Cells_per_CM3, config);
        }

        public static CellDensity New(double value, CellDensityUnit unit,
            IQuantityConfig<CellDensityUnit> config = null)
        {
            return new CellDensity(value, unit, config);
        }

        private string GetUnitAsString()
        {
            switch (this.Unit)
            {
                case CellDensityUnit.Cells_per_CM3:
                    return string.Format("cells/cm3");
                case CellDensityUnit.Cells_per_MM3:
                    return string.Format("cells/mm3");
                default:
                    return string.Format("???");
            }
        }

        private string GetValueAsString()
        {
            if (double.IsNaN(this.Value))
                return "N/A";
            return string.Format(string.Format("{{0:E{0:D}{1}", (object)this._core.DecimalDigits, (object)"}"),
                (object)this.Value);
        }


        public override int GetHashCode() => _core.GetHashCode();

        public double ValueAs(string unit)
        {
            if (this.UnitAsString.ToUpper().Contains(unit.ToUpper()))
            {
                return Value;
            }

            if (unit.ToUpper().Contains("CM3") || unit.ToUpper().Contains("CC"))
            {
                return Value * 1000;
            }
            else
            {
                return Value / 1000;
            }
        }
    }
}