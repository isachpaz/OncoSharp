// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Dose;

namespace OncoSharp.Core.Quantities.Extensions
{
    public static class DoseExtensions
    {
        public static DoseValue Gy(this double doseInGy)
        {
            return DoseValue.InGy(doseInGy);
        }

        public static EQD2Value Gy_Eqd2(this double doseInGy)
        {
            return EQD2Value.InGy(doseInGy);
        }

        public static DoseValue Gy(this int doseInGy)
        {
            return DoseValue.InGy((double)doseInGy);
        }

        public static EQD2Value Gy_Eqd2(this int doseInGy)
        {
            return EQD2Value.InGy(doseInGy);
        }

        public static EQD0Value Gy_Eqd0(this double doseInGy)
        {
            return EQD0Value.InGy(doseInGy);
        }
        
        public static EQD0Value Gy_Eqd0(this int doseInGy)
        {
            return EQD0Value.InGy(doseInGy);
        }
    }
}