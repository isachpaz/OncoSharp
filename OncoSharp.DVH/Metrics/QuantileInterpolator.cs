// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.DVH.Metrics
{
    public class QuantileInterpolator : IDVHInterpolator
    {
        private readonly IReadOnlyList<double> _rawSamples;
        private readonly QuantileCalculator _calculator;

        public QuantileInterpolator(IReadOnlyList<double> rawSamples, QuantileCalculator calculator)
        {
            _rawSamples = rawSamples ?? throw new ArgumentNullException(nameof(rawSamples));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        public double GetDoseAtVolume(VolumeValue volume)
        {
            double quantile = 1.0 - volume.Value / 100.0;
            return _calculator.QuantileType7(_rawSamples, quantile);
        }

        public VolumeValue GetVolumeAtDose(double dose, VolumeUnit unit)
        {
            int count = _rawSamples.Count(v => v >= dose);

            double frac = count / (double)_rawSamples.Count;

            switch (unit)
            {
                case VolumeUnit.PERCENT:
                    return VolumeValue.InPercent(frac * 100.0);
                default:
                    throw new NotSupportedException("Only percent is supported for raw sample volume calculation.");
            }
        }
    }
}