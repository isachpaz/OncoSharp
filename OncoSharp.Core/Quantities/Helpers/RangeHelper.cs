// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using OncoSharp.Core.Quantities.Interfaces;

namespace OncoSharp.Core.Quantities.Helpers
{
    public static class QuantityRange<TValue, TUnit>
        where TValue : struct, IQuantityCreation<TValue, TUnit>, IQuantityGetters<TValue, TUnit>
        where TUnit : Enum
    {
        public static IEnumerable<TValue> Range(TValue start, TValue stop, TValue step, bool includeLast = false)
        {
            if (!start.GetUnits().Equals(stop.GetUnits()) || !start.GetUnits().Equals(step.GetUnits()))
                throw new InvalidOperationException("Units must match.");

            var current = start.GetValue();
            var end = stop.GetValue();
            var stepSize = step.GetValue();

            while (current < end)
            {
                yield return new TValue().TNew(current, start.GetUnits());
                current += stepSize;
            }

            if (includeLast)
                yield return stop;
        }
    }
}