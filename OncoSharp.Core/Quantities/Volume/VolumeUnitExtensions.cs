// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.Volume
{
    public static class VolumeUnitExtensions
    {
        public static string ToSymbol(this VolumeUnit unit)
        {
            switch (unit)
            {
                case VolumeUnit.MM3:
                    return "mm³";
                case VolumeUnit.CM3:
                    return "cm³";
                case VolumeUnit.PERCENT:
                    return "%";
                default:
                    return "???";
            }
        }

        public static VolumeUnit Parse(string unitStr)
        {
            switch (unitStr.Trim().ToLowerInvariant())
            {
                case "mm3":
                case "mm³":
                    return VolumeUnit.MM3;
                case "cm3":
                case "cm³":
                case "cc":
                    return VolumeUnit.CM3;
                case "%":
                    return VolumeUnit.PERCENT;
                default:
                    return VolumeUnit.UNKNOWN;
            }
        }
    }
}