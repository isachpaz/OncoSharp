// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;

namespace OncoSharp.Radiobiology.LQ
{
    public class LqFractionated : IEquieffectiveDoseConverter
    {
        public double AlphaBetaRatio { get; }
        public double NumberOfFraction { get; }
        public double NearToZeroAlphaBetaCutoff { get; }
        public double LargeAlphaBetaCutoff { get; }

        public LqFractionated(double alphaBetaRatio,
            double numberOfFraction,
            double nearToZeroAlphaBetaCutoff = 1e-3,
            double largeAlphaBetaCutoff = 1e6)
        {
            AlphaBetaRatio = alphaBetaRatio;
            NumberOfFraction = numberOfFraction;
            NearToZeroAlphaBetaCutoff = nearToZeroAlphaBetaCutoff;

            LargeAlphaBetaCutoff = largeAlphaBetaCutoff;
        }

        public double ComputeEqd0(double totalDose)
        {
            return totalDose * (1.0 + (totalDose / NumberOfFraction) / AlphaBetaRatio);
        }

        public double ComputeEqd2(double totalDose)
        {
            if (IsAlphaBetaRatioNearToZero())
            {
                return totalDose * totalDose / 2.0 / NumberOfFraction;
            }

            if (IsAlphaBetaRatioTooLarge())
            {
                return totalDose;
            }

            return ComputeEqd0(totalDose) / (1.0 + 2.0/AlphaBetaRatio);
        }

        private bool IsAlphaBetaRatioNearToZero() => Math.Abs(AlphaBetaRatio - 0.0) <= NearToZeroAlphaBetaCutoff;
        private bool IsAlphaBetaRatioTooLarge() => Math.Abs(AlphaBetaRatio) >= LargeAlphaBetaCutoff;
    }
}