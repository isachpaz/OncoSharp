// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;

namespace OncoSharp.DVH.Metrics
{
    public partial class DVHMetricCalculator
    {
        public static class Factory
        {
            public static DVHMetricCalculator QuantileType7(IDVHBase dvh)
            {
                IReadOnlyList<double> samples;
                if (dvh.IsRawDoseSamplesAvailable)
                {
                    samples = dvh.RawDoseSamples;
                    return new DVHMetricCalculator(new QuantileInterpolator(samples, new QuantileCalculator())
                    );
                }
                else
                {
                    return new DVHMetricCalculator(new DVHBinInterpolator((DVHBase)dvh.ToCumulative()));
                }
            }
        }
    }
}