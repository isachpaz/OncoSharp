// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.Dose
{
    public class DoseConfig : IQuantityConfig<DoseUnit>
    {
        public virtual int Decimals(DoseUnit unit)
        {
            switch (unit)
            {
                case DoseUnit.cGy:
                    return 2;
                case DoseUnit.Gy:
                    return 2;
                case DoseUnit.PERCENT:
                    return 2;
                default:
                    return 2;
            }
        }

        public virtual double Error()
        {
            // Defines a default epsilon for dose equality comparison
            return 1e-6;
        }

        public static DoseConfig Default() => new DoseConfig();
    }
}