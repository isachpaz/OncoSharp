// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.DimensionlessValues
{
    public class DoubleConfig : IQuantityConfig<UnitLess>
    {
        public virtual int Decimals(UnitLess unit) => 6;

        public virtual double Error() => 1e-12;

        public static DoubleConfig Default() => new DoubleConfig();
    }
}