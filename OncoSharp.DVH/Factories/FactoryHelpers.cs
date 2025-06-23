// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH.Factories
{
    public static class FactoryHelpers
    {
        public static List<DVHPoint> ToAbsoluteVolumeValue(this List<DVHPoint> dvhPoints, VolumeValue volume)
        {
            var first = dvhPoints.First();
            if (first.Volume.Unit != VolumeUnit.PERCENT)
                return dvhPoints;

            return dvhPoints.Select(p =>
                new DVHPoint(p.Dose, VolumeValue.New(p.Volume.Value * volume.Value / 100.0, volume.Unit))).ToList();
        }
    }
}