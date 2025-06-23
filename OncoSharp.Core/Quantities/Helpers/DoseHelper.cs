// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using OncoSharp.Core.Quantities.Dose;

namespace OncoSharp.Core.Quantities.Helpers
{
    public static class DoseHelper
    {
        public static IEnumerable<DoseValue> PhysicalDoseRange(double start, double stop, double step,
            bool bIncludeLastValue,
            DoseUnit unit)
        {
            return QuantityRange<DoseValue, DoseUnit>.Range(
                DoseValue.New(start, unit),
                DoseValue.New(stop, unit),
                DoseValue.New(step, unit),
                bIncludeLastValue);
        }

        //public static IEnumerable<EQD2Value> EQD2Range(double start, double stop, double step, bool bIncludeLastValue, DoseUnit unit)
        //{
        //    return RangeHelper<EQD2Value>.Range(
        //        EQD2Value.New(start, unit),
        //        EQD2Value.New(stop, unit),
        //        EQD2Value.New(step, unit),
        //        bIncludeLastValue);
        //}

        //public static IEnumerable<EQD0Value> EQD0Range(double start, double stop, double step, bool bIncludeLastValue, DoseUnit unit)
        //{
        //    return RangeHelper<EQD0Value>.Range(
        //        EQD0Value.New(start, unit),
        //        EQD0Value.New(stop, unit),
        //        EQD0Value.New(step, unit),
        //        bIncludeLastValue);
        //}

        //public static IEnumerable<GammaValue> GammaRange(double start, double stop, double step, bool bIncludeLastValue)
        //{
        //    return RangeHelper<GammaValue>.Range(
        //        GammaValue.New(start),
        //        GammaValue.New(stop),
        //        GammaValue.New(step),
        //        bIncludeLastValue);
        //}
    }
}