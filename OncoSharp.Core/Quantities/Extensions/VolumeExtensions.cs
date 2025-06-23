// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.Core.Quantities.Extensions
{
    public static class VolumeExtensions
    {
        public static VolumeValue mm3(this double volume)
        {
            return VolumeValue.New(volume, VolumeUnit.MM3);
        }

        public static VolumeValue cm3(this double volume)
        {
            return VolumeValue.New(volume, VolumeUnit.CM3);
        }

        public static VolumeValue mm3(this float volume)
        {
            return VolumeValue.New(volume, VolumeUnit.MM3);
        }

        public static VolumeValue cm3(this float volume)
        {
            return VolumeValue.New(volume, VolumeUnit.CM3);
        }
        
        public static VolumeValue mm3(this int volume)
        {
            return VolumeValue.New(volume, VolumeUnit.MM3);
        }

        public static VolumeValue cm3(this int volume)
        {
            return VolumeValue.New(volume, VolumeUnit.CM3);
        }
    }
}