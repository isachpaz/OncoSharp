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
    public readonly struct FractionsValue :
        IComparable<FractionsValue>,
        IEquatable<FractionsValue>,
        IQuantityCreation<FractionsValue, UnitLess>,
        IQuantityGetters<FractionsValue, UnitLess>
    {
        private readonly QuantityCore<UnitLess> _core;

        public double Value => _core.Value;
        public UnitLess Unit => _core.Unit;

        public FractionsValue(double value, IQuantityConfig<UnitLess> config = null)
        {
            var checkedValue = QuantityValidation.EnsurePositiveOrThrowException(value, nameof(value));
            config = config ?? FractionsConfig.Default();

            _core = new QuantityCore<UnitLess>(checkedValue, default,
                config.Decimals(default),
                config.Error());
        }

        public int CompareTo(FractionsValue other)
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

        public static FractionsValue operator -(FractionsValue fv1, FractionsValue fv2)
        {
            return new FractionsValue(fv1.Value - fv2.Value);
        }

        public static FractionsValue operator +(FractionsValue fv1, FractionsValue fv2)
        {
            return new FractionsValue(fv1.Value + fv2.Value);
        }

        //public override string ToString() => $"{nameof(Value)}: {Value:F2}";

        // Core operations
        public static FractionsValue Zero => new FractionsValue(0);
        public static FractionsValue Invalid => new FractionsValue(double.NaN);
        public bool IsValid => !double.IsNaN(Value);

        public override string ToString() =>
            IsValid ? (Value % 1 == 0 ? Value.ToString("0") : Value.ToString("0.##")) : "Invalid";

        
        public static FractionsValue Empty()
        {
            return new FractionsValue(double.NaN);
        }

        public bool Equals(FractionsValue other)
        {
            return Math.Abs(this.Value - other.Value) < _core.Error;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FractionsValue other && Equals(other);
        }

        public double GetValue() => Value;
        UnitLess IQuantityGetters<FractionsValue, UnitLess>.GetUnits() => _core.Unit;

        public string GetUnits() => String.Empty;

        public override int GetHashCode() => _core.GetHashCode();


        public FractionsValue TNew(double value, UnitLess unit = UnitLess.UNITLESS)
        {
            return new FractionsValue(value);
        }

       
        public static implicit operator FractionsValue(double value)
        {
            return new FractionsValue(value);
        }

        public static implicit operator Double(FractionsValue value)
        {
            return value.Value;
        }
    }
}