// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.DimensionlessValues;
using OncoSharp.Core.Quantities.Interfaces;
using OncoSharp.Core.Quantities.Probability;

namespace OncoSharp.Core.Quantities.Fractions
{
    public readonly struct FractionsNumberValue :
        IComparable<FractionsNumberValue>,
        IEquatable<FractionsNumberValue>,
        IQuantityCreation<FractionsNumberValue, UnitLess>,
        IQuantityGetters<FractionsNumberValue, UnitLess>
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public FractionsNumberValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));
            config = config ?? ProbabilityConfig.Default();

            _core = new QuantityCore<UnitLess>(checkedValue, default,
                config.Decimals(default),
                config.Error());
        }

        public int CompareTo(FractionsNumberValue other)
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

        public static FractionsNumberValue operator -(FractionsNumberValue fv1, FractionsNumberValue fv2)
        {
            return new FractionsNumberValue(fv1.Value - fv2.Value);
        }

        public static FractionsNumberValue operator +(FractionsNumberValue fv1, FractionsNumberValue fv2)
        {
            return new FractionsNumberValue(fv1.Value + fv2.Value);
        }

        public static FractionsNumberValue Zero => new FractionsNumberValue(0);

        public static FractionsNumberValue Empty()
        {
            return new FractionsNumberValue(double.NaN);
        }

        public bool Equals(FractionsNumberValue other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FractionsNumberValue other && Equals(other);
        }

        public double GetValue() => Value;
        UnitLess IQuantityGetters<FractionsNumberValue, UnitLess>.GetUnits() => _core.Unit;

        public string GetUnits() => String.Empty;

        public override int GetHashCode() => _core.GetHashCode();


        public FractionsNumberValue TNew(double value, UnitLess unit = UnitLess.UNITLESS)
        {
            return new FractionsNumberValue(value);
        }

        public override string ToString() => $"{nameof(Value)}: {Value:F2}";

        public static implicit operator FractionsNumberValue(double value)
        {
            return new FractionsNumberValue(value);
        }

        public static implicit operator Double(FractionsNumberValue value)
        {
            return value.Value;
        }
    }
}