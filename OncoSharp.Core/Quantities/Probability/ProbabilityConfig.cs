// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.DimensionlessValues;

namespace OncoSharp.Core.Quantities.Probability
{
    public class ProbabilityConfig : IQuantityConfig<UnitLess>
    {
        private readonly int _decimals;
        private readonly double _error;

        public ProbabilityConfig(int decimals = 4, double error = 1e-6)
        {
            _decimals = decimals;
            _error = error;
        }

        public int Decimals(UnitLess unit)
        {
            return _decimals;
        }

        public double Error() => _error;

        public static ProbabilityConfig Default() => new ProbabilityConfig();
    }
}