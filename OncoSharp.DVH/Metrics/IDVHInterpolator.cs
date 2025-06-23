// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH.Metrics
{
    public interface IDVHInterpolator
    {
        double GetDoseAtVolume(VolumeValue volume);
        VolumeValue GetVolumeAtDose(double dose, VolumeUnit unit);
    }
}