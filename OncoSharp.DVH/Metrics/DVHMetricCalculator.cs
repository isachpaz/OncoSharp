// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH.Metrics
{
    public partial class DVHMetricCalculator
    {
        private readonly IDVHInterpolator _interpolator;

        public DVHMetricCalculator(IDVHInterpolator interpolator)
        {
            _interpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
        }

        public double GetDoseAtVolume(VolumeValue volume) => _interpolator.GetDoseAtVolume(volume);

        public VolumeValue GetVolumeAtDose(double dose, VolumeUnit unit) => _interpolator.GetVolumeAtDose(dose, unit);
    }
}