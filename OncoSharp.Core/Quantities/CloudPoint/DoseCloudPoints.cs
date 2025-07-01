// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Interfaces;
using OncoSharp.Core.Quantities.Volume;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OncoSharp.Core.Quantities.CloudPoint
{
    public sealed class DoseCloudPoints<TDose>
        where TDose : struct, IComparable<TDose>,
        IQuantityCreation<TDose, DoseUnit>,
        IQuantityArithmetic<TDose>,
        IQuantityGetters<TDose, DoseUnit>
    {
        private readonly IReadOnlyList<DoseCloudPoint<TDose>> _voxelDoses;
        private readonly Lazy<VolumeValue> _totalVolume;

        public DoseCloudPoints(IList<DoseCloudPoint<TDose>> voxelDoses)
        {
            var volumeUnits = voxelDoses.FirstOrDefault().Volume.Unit;

            _voxelDoses = new List<DoseCloudPoint<TDose>>(voxelDoses).AsReadOnly();
            _totalVolume = new Lazy<VolumeValue>(() =>
                new VolumeValue(_voxelDoses.Sum(p=>p.Volume.Value), volumeUnits));
        }

        public IReadOnlyList<DoseCloudPoint<TDose>> VoxelDoses => _voxelDoses;
        public VolumeValue TotalVolume => _totalVolume.Value;
    }
}