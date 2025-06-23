// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Helpers;

namespace OncoSharp.DVH.Metrics
{
    public class DVHBinInterpolator : IDVHInterpolator
    {
        private readonly DVHBase _dvh;

        public DVHBinInterpolator(DVHBase dvh)
        {
            _dvh = dvh ?? throw new ArgumentNullException(nameof(dvh));
        }

        public double GetDoseAtVolume(VolumeValue volume)
        {
            if (!_dvh.IsNormalized && volume.Unit == VolumeUnit.PERCENT)
            {
                return GetDoseAtVolume(volume.Value * _dvh.TotalVolume.Value / 100.0);
            }

            return GetDoseAtVolume(volume.Value);
        }

        private double GetDoseAtVolume(double volume)
        {
            var curve = _dvh.DVHCurve;
            var minVol = curve.Min(p => p.Volume.Value);
            var maxVol = curve.Max(p => p.Volume.Value);

            if (volume <= minVol) return _dvh.MaxDose;
            if (volume == maxVol) return _dvh.MinDose;
            if (volume > maxVol) return double.NaN;

            var closest = curve.OrderBy(p => Math.Abs(p.Volume.Value - volume)).First();
            if (Math.Abs(closest.Volume.Value - volume) < 0.001) return closest.Dose;

            var list = curve.ToList();
            var index1 = list.IndexOf(closest);
            var index2 = closest.Volume.Value < volume ? index1 - 1 : index1 + 1;

            if (index1 >= 0 && index2 >= 0 && index2 < list.Count)
            {
                var p1 = list[index1];
                var p2 = list[index2];
                return MathHelper.Interpolate(p1.Volume.Value, p2.Volume.Value, p1.Dose, p2.Dose, volume);
            }

            return double.NaN;
        }

        public double GetDoseComplement(VolumeValue volume)
        {
            var maxVol = _dvh.DVHCurve.Max(p => p.Volume);
            return GetDoseAtVolume((maxVol - volume).Value);
        }

        public VolumeValue GetVolumeAtDose(double doseVal, VolumeUnit unit)
        {
            var curve = _dvh.DVHCurve.Select(p => new { p.Dose, Volume = p.Volume.Value, p.Volume.Unit }).ToList();
            var minDose = curve.Min(p => p.Dose);
            var maxDose = curve.Max(p => p.Dose);

            if (doseVal >= maxDose) return VolumeValue.New(0.0, unit);
            if (doseVal < minDose) return _dvh.DVHCurve.Max(p => p.Volume);

            var lower = curve.Last(p => p.Dose <= doseVal);
            var higher = curve.First(p => p.Dose > doseVal);

            var volumeAtDose = MathHelper.Interpolate(higher.Dose, lower.Dose, higher.Volume, lower.Volume, doseVal);
            return VolumeValue.New(volumeAtDose, unit);
        }

        public VolumeValue GetComplementVolumeAtDose(double doseVal, VolumeUnit unit)
        {
            var totalVolume = _dvh.DVHCurve.Max(p => p.Volume);
            var volumeAtDose = GetVolumeAtDose(doseVal, unit);
            return totalVolume - volumeAtDose;
        }

        public double MaxDose()
        {
            return _dvh.DVHCurve.Any() ? _dvh.DVHCurve.Max(p => p.Dose) : double.NaN;
        }

        public double MinDose()
        {
            return _dvh.DVHCurve.Any() ? _dvh.DVHCurve.Min(p => p.Dose) : double.NaN;
        }

        public double MeanDose()
        {
            if (!_dvh.DVHCurve.Any()) return double.NaN;

            var ddvh = ToDifferential(_dvh.DVHCurve.ToArray());
            var weightedSum = ddvh.Sum(p => p.Dose * p.Volume.Value);
            var totalVolume = ddvh.Sum(p => p.Volume.Value);

            return weightedSum / totalVolume;
        }

        public DVHPoint[] ToDifferential(DVHPoint[] cdvh)
        {
            if (cdvh == null || cdvh.Length < 2)
                return Array.Empty<DVHPoint>();

            var volumeUnit = cdvh.First().Volume.Unit;
            var differential = new List<DVHPoint>(cdvh.Length - 1);

            for (int i = 0; i < cdvh.Length - 1; i++)
            {
                var v = cdvh[i].Volume.Value - cdvh[i + 1].Volume.Value;
                var d = cdvh[i + 1].Dose - cdvh[i].Dose;
                var dvhVal = v / d;
                differential.Add(new DVHPoint(cdvh[i].Dose, new VolumeValue(dvhVal, volumeUnit)));
            }

            var max = differential.Max(p => p.Volume.Value);
            for (int i = 0; i < differential.Count; i++)
            {
                var p = differential[i];
                differential[i] = new DVHPoint(p.Dose, new VolumeValue(p.Volume.Value / max, p.Volume.Unit));
            }

            return differential.ToArray();
        }

        public double GetDoseAtVolume(VolumeValue volume, out bool interpolated)
        {
            var result = GetDoseAtVolume(volume.Value);
            interpolated = true; // You can enhance this method to detect if it was truly interpolated
            return result;
        }
    }
}