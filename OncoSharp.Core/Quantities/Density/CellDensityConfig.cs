// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.Density
{
    public class CellDensityConfig : IQuantityConfig<CellDensityUnit>
    {
        public CellDensityConfig()
        {
        }

        int IQuantityConfig<CellDensityUnit>.Decimals(CellDensityUnit unit)
        {
            switch (unit)
            {
                case CellDensityUnit.Cells_per_CM3:
                    return 5;
                case CellDensityUnit.Cells_per_MM3:
                    return 5;
                default:
                    return 5;
            }
        }

        public double Error() => 1e-9;

        public static CellDensityConfig Default() => new CellDensityConfig();
    }
}