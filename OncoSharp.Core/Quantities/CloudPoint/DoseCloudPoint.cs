// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Interfaces;
using System;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.Core.Quantities.CloudPoint
{
    public struct DoseCloudPoint<TDose>
        where TDose : struct, IComparable<TDose>, IQuantityCreation<TDose, DoseUnit>, IQuantityArithmetic<TDose>,
        IQuantityGetters<TDose, DoseUnit>
    {
        public TDose Dose { get; }
        public VolumeValue Volume { get; }

        public DoseCloudPoint(TDose dose, VolumeValue volume)
        {
            Dose = dose;
            Volume = volume;
        }
    }
}