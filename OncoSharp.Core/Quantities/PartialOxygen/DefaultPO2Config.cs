// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.PartialOxygen
{
    public class DefaultPO2Config : IQuantityConfig<PO2Unit>
    {
        public int Decimals(PO2Unit unit)
        {
            switch (unit)
            {
                case PO2Unit.mmHg:
                    return 3;
                default:
                    return 3;
            }
        }

        public double Error() => 1E-3;
    }
}