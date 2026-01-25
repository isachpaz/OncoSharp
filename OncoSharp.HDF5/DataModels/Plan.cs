// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Fractions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.RTDomainModel;

namespace OncoSharp.HDF5.DataModels
{
    public sealed class Plan : IPlanItem
    {
        private const double NearToZeroAlphaBetaCutoff = 1e-3;
        private const double LargeAlphaBetaCutoff = 1e6;

        public string PatientId { get; internal set; }
        public string PlanId { get; }

        // /rois/<roi>
        private readonly Dictionary<string, Roi> _rois =
            new Dictionary<string, Roi>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<Roi> Rois => _rois.Values;

        // Store plan-level metadata (fractions, techniques, etc.)
        public Dictionary<string, string> Attributes { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Plan(string planId)
            : this(planId, null)
        {
        }

        public Plan(string planId, string patientId)
        {
            PlanId = planId;
            PatientId = patientId;
        }

        public DoseValue PrescriptionDose =>
            TryGetDoseAttribute("PrescriptionDose", out var dose) ||
            TryGetDoseAttribute("TotalDose", out dose)
                ? dose
                : DoseValue.Empty();

        public FractionsValue Fractions => TryGetFractions(out var fractions)
            ? fractions
            : FractionsValue.Empty();

        public bool IsValid =>
            Rois.Any(roi => roi.Dose != null) && Fractions.IsValid;

        public Roi GetOrAddRoi(string roiName)
        {
            Roi roi;
            if (!_rois.TryGetValue(roiName, out roi))
            {
                roi = new Roi(roiName);
                _rois[roiName] = roi;
            }

            return roi;
        }

        public IReadOnlyList<string> GetRoiNames()
        {
            if (_rois.Count == 0)
                return Array.Empty<string>();

            return _rois.Keys
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool TryGetRoi(string roiName, out Roi roi) =>
            _rois.TryGetValue(roiName, out roi);

        public DoseCloudPoints<EQD2Value> CalculateEqd2DoseDistribution(string structureId, DoseValue abRatio)
        {
            if (!TryGetDoseRef(structureId, out var doseRef))
                return CreateInvalidEqd2Cloud();

            var fractions = Fractions;
            if (!fractions.IsValid || fractions.Value <= 0)
                return CreateInvalidEqd2Cloud();

            var doseValues = doseRef.Values ?? Array.Empty<double>();
            if (doseValues.Length == 0)
                return CreateInvalidEqd2Cloud();
            var voxelVolume = ResolveVoxelVolume(doseRef);
            var doseUnit = ParseDoseUnit(doseRef.Units);
            var abRatioGy = ConvertDoseToGy(abRatio);

            var points = new List<DoseCloudPoint<EQD2Value>>(doseValues.Length);
            foreach (var dose in doseValues)
            {
                var doseGy = ConvertDoseToGy(dose, doseUnit);
                var eqd2Gy = ComputeEqd2Gy(doseGy, fractions.Value, abRatioGy);
                points.Add(new DoseCloudPoint<EQD2Value>(EQD2Value.InGy(eqd2Gy), voxelVolume));
            }

            return new DoseCloudPoints<EQD2Value>(points);
        }

        public DoseCloudPoints<EQD0Value> CalculateEqd0DoseDistribution(string structureId, DoseValue abRatio)
        {
            if (!TryGetDoseRef(structureId, out var doseRef))
                return CreateInvalidEqd0Cloud();

            var fractions = Fractions;
            if (!fractions.IsValid || fractions.Value <= 0)
                return CreateInvalidEqd0Cloud();

            var doseValues = doseRef.Values ?? Array.Empty<double>();
            if (doseValues.Length == 0)
                return CreateInvalidEqd0Cloud();
            var voxelVolume = ResolveVoxelVolume(doseRef);
            var doseUnit = ParseDoseUnit(doseRef.Units);
            var abRatioGy = ConvertDoseToGy(abRatio);

            var points = new List<DoseCloudPoint<EQD0Value>>(doseValues.Length);
            foreach (var dose in doseValues)
            {
                var doseGy = ConvertDoseToGy(dose, doseUnit);
                var eqd0Gy = ComputeEqd0Gy(doseGy, fractions.Value, abRatioGy);
                points.Add(new DoseCloudPoint<EQD0Value>(EQD0Value.InGy(eqd0Gy), voxelVolume));
            }

            return new DoseCloudPoints<EQD0Value>(points);
        }

        private bool TryGetFractions(out FractionsValue fractions)
        {
            fractions = FractionsValue.Empty();
            if (!Attributes.TryGetValue("FractionsNumber", out var raw) ||
                string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
                value <= 0)
            {
                return false;
            }

            fractions = new FractionsValue(value);
            return true;
        }

        private bool TryGetDoseAttribute(string key, out DoseValue dose)
        {
            dose = DoseValue.Empty();
            if (!Attributes.TryGetValue(key, out var raw) ||
                string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return false;

            dose = DoseValue.InGy(value);
            return true;
        }

        private bool TryGetDoseRef(string structureId, out DoseRef doseRef)
        {
            doseRef = null;
            if (string.IsNullOrWhiteSpace(structureId))
                return false;

            if (!TryGetRoi(structureId, out var roi) || roi.Dose == null)
                return false;

            doseRef = roi.Dose;
            return true;
        }

        private static double ConvertDoseToGy(DoseValue dose)
        {
            switch (dose.Unit)
            {
                case DoseUnit.cGy:
                    return dose.Value / 100.0;
                case DoseUnit.Gy:
                    return dose.Value;
                default:
                    return dose.Value;
            }
        }

        private static double ConvertDoseToGy(double dose, DoseUnit unit)
        {
            switch (unit)
            {
                case DoseUnit.cGy:
                    return dose / 100.0;
                case DoseUnit.Gy:
                    return dose;
                default:
                    return dose;
            }
        }

        private static double ComputeEqd0Gy(double totalDoseGy, double fractions, double abRatioGy)
        {
            if (double.IsNaN(totalDoseGy) || double.IsNaN(fractions) || double.IsNaN(abRatioGy))
                return double.NaN;

            if (fractions <= 0 || abRatioGy <= 0)
                return double.NaN;

            return totalDoseGy * (1.0 + (totalDoseGy / fractions) / abRatioGy);
        }

        private static double ComputeEqd2Gy(double totalDoseGy, double fractions, double abRatioGy)
        {
            if (double.IsNaN(totalDoseGy) || double.IsNaN(fractions) || double.IsNaN(abRatioGy))
                return double.NaN;

            if (fractions <= 0)
                return double.NaN;

            var absAbRatio = Math.Abs(abRatioGy);
            if (absAbRatio <= NearToZeroAlphaBetaCutoff)
                return (totalDoseGy * totalDoseGy) / (2.0 * fractions);

            if (absAbRatio >= LargeAlphaBetaCutoff)
                return totalDoseGy;

            var eqd0Gy = ComputeEqd0Gy(totalDoseGy, fractions, abRatioGy);
            return double.IsNaN(eqd0Gy) ? double.NaN : eqd0Gy / (1.0 + 2.0 / abRatioGy);
        }

        private static VolumeValue ResolveVoxelVolume(DoseRef doseRef)
        {
            if (doseRef?.VoxelVolume == null)
                return VolumeValue.Empty();

            var unit = ParseVolumeUnit(doseRef.VolumeUnits);
            var value = doseRef.VoxelVolume.Value;
            return VolumeValue.New(value, unit);
        }

        private static VolumeUnit ParseVolumeUnit(string units)
        {
            if (string.IsNullOrWhiteSpace(units))
                return VolumeUnit.CM3;

            switch (units.Trim().ToLowerInvariant())
            {
                case "cm3":
                case "cm^3":
                case "cc":
                    return VolumeUnit.CM3;
                case "mm3":
                case "mm^3":
                    return VolumeUnit.MM3;
                case "%":
                case "percent":
                    return VolumeUnit.PERCENT;
                default:
                    return VolumeUnit.UNKNOWN;
            }
        }

        private static DoseUnit ParseDoseUnit(string units)
        {
            if (string.IsNullOrWhiteSpace(units))
                return DoseUnit.Gy;

            switch (units.Trim().ToLowerInvariant())
            {
                case "gy":
                    return DoseUnit.Gy;
                case "cgy":
                    return DoseUnit.cGy;
                case "%":
                case "percent":
                    return DoseUnit.PERCENT;
                default:
                    return DoseUnit.UNKNOWN;
            }
        }

        private static DoseCloudPoints<EQD2Value> CreateInvalidEqd2Cloud()
        {
            return new DoseCloudPoints<EQD2Value>(new[]
            {
                new DoseCloudPoint<EQD2Value>(EQD2Value.Empty(), VolumeValue.Empty())
            });
        }

        private static DoseCloudPoints<EQD0Value> CreateInvalidEqd0Cloud()
        {
            return new DoseCloudPoints<EQD0Value>(new[]
            {
                new DoseCloudPoint<EQD0Value>(EQD0Value.Empty(), VolumeValue.Empty())
            });
        }
    }
}
