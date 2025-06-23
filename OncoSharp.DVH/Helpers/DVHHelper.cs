// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.DVH.Decorators;

namespace OncoSharp.DVH.Helpers
{
    public static class DVHHelper
    {
        public static IDVHBase NormalizeVolume(this IDVHBase dvh) => new VolumeNormalization(dvh);
    }
}