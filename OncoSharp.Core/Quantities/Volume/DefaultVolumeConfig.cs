// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.Volume
{
    public class VolumeConfig : IQuantityConfig<VolumeUnit>
    {
        private readonly double _error;

        public VolumeConfig(double error = 1e-6)
        {
            _error = error;
        }

        public virtual int Decimals(VolumeUnit unit)
        {
            switch (unit)
            {
                case VolumeUnit.MM3:
                    return 2;
                case VolumeUnit.CM3:
                    return 2;
                case VolumeUnit.PERCENT:
                    return 2;
                default:
                    return 2;
            }
        }

        public double Error() => _error;

        public static VolumeConfig Default() => new VolumeConfig();
    }
}