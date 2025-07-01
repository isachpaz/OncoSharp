// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.DimensionlessValues;
using OncoSharp.Core.Quantities.Probability;

namespace OncoSharp.Core.Quantities.Fractions
{
    public class FractionsConfig : IQuantityConfig<UnitLess>
    {
        private readonly int _decimals;
        private readonly double _error;

        public FractionsConfig(int decimals = 4, double error = 1e-6)
        {
            _decimals = decimals;
            _error = error;
        }

        public int Decimals(UnitLess unit)
        {
            return _decimals;
        }

        public double Error() => _error;

        public static FractionsConfig Default() => new FractionsConfig();
    }
}