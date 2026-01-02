// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.Radiobiology.LQ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OncoSharp.Radiobiology.GEUD
{
    public class Geud2GyModel
    {
        public double AlphaVolumeEffect { get; }

        public Geud2GyModel(double alphaVolumeEffect)
        {
            AlphaVolumeEffect = alphaVolumeEffect;
        }

        public static Geud2GyModel Create(double alphaVolumeEffect)
        {
            return new Geud2GyModel(alphaVolumeEffect);
        }

        public EQD2Value Calculate(DoseCloudPoints<EQD2Value> points)
        {
            var items = points;
            //var totalVolume = items.Select(x => x.Volume).Aggregate((v1, v2) => v1 + v2);
            var totalVolume = points.TotalVolume;
            var units = points.DoseUnit;
            double gEUD = 0.0;
            foreach (var item in items.VoxelDoses)
            {
                var dose = item.Dose;
                var volume = item.Volume;
                var fractionalVolume = volume / totalVolume;
                gEUD += fractionalVolume * Math.Pow(dose.Value, AlphaVolumeEffect);
            }
            return EQD2Value.New(Math.Pow(gEUD, 1.0 / AlphaVolumeEffect), units);
        }

        protected DoseUnit GetDoseUnit(IReadOnlyList<DoseCloudPoint<EQD2Value>> points)
        {
            var doseUnit = points?.FirstOrDefault().Dose.Unit;
            if (doseUnit != null) return (DoseUnit)doseUnit;
            throw new InvalidDataException("You cannot reach this point.");
        }

    }
}