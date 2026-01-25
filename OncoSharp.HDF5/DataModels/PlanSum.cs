// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Fractions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.RTDomainModel;

namespace OncoSharp.HDF5.DataModels
{
    public sealed class PlanSum : ICompositePlan
    {
        private readonly List<IPlanItem> _plans = new List<IPlanItem>();
        private string _patientId;

        public string PlanSumId { get; }
        public string PlanId => PlanSumId;
        public string PatientId => _patientId ?? _plans.FirstOrDefault()?.PatientId;

        // Store arbitrary attributes (component ids, weights, HDF5 paths, etc.)
        public Dictionary<string, string> Attributes { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public PlanSum(string planSumId)
            : this(planSumId, null)
        {
        }

        public PlanSum(string planSumId, string patientId)
        {
            PlanSumId = planSumId;
            _patientId = patientId;
        }

        public DoseValue PrescriptionDose =>
            _plans.Count > 0 ? _plans[0].PrescriptionDose : DoseValue.Empty();

        public FractionsValue Fractions =>
            _plans.Count > 0 ? _plans[0].Fractions : FractionsValue.Empty();

        public bool IsValid => _plans.Count > 0 && _plans.All(plan => plan.IsValid);

        public DoseCloudPoints<EQD2Value> CalculateEqd2DoseDistribution(string structureId, DoseValue abRatio)
        {
            if (_plans.Count == 0)
                return CreateInvalidEqd2Cloud();

            var baseDistribution = _plans[0].CalculateEqd2DoseDistribution(structureId, abRatio);
            if (baseDistribution.VoxelDoses.Count == 0)
                return baseDistribution;

            var count = baseDistribution.VoxelDoses.Count;
            var voxelVolume = baseDistribution.VoxelDoses[0].Volume;
            var doseUnit = baseDistribution.DoseUnit;
            var accumulated = new double[count];

            for (int i = 0; i < count; i++)
                accumulated[i] = baseDistribution.VoxelDoses[i].Dose.Value;

            for (int p = 1; p < _plans.Count; p++)
            {
                var current = _plans[p].CalculateEqd2DoseDistribution(structureId, abRatio);
                if (current.VoxelDoses.Count != count || current.DoseUnit != doseUnit)
                    throw new InvalidOperationException("Plan sum dose grids are not compatible.");

                for (int i = 0; i < count; i++)
                    accumulated[i] += current.VoxelDoses[i].Dose.Value;
            }

            var points = new List<DoseCloudPoint<EQD2Value>>(count);
            for (int i = 0; i < count; i++)
                points.Add(new DoseCloudPoint<EQD2Value>(EQD2Value.New(accumulated[i], doseUnit), voxelVolume));

            return new DoseCloudPoints<EQD2Value>(points);
        }

        public DoseCloudPoints<EQD0Value> CalculateEqd0DoseDistribution(string structureId, DoseValue abRatio)
        {
            if (_plans.Count == 0)
                return CreateInvalidEqd0Cloud();

            var baseDistribution = _plans[0].CalculateEqd0DoseDistribution(structureId, abRatio);
            if (baseDistribution.VoxelDoses.Count == 0)
                return baseDistribution;

            var count = baseDistribution.VoxelDoses.Count;
            var voxelVolume = baseDistribution.VoxelDoses[0].Volume;
            var doseUnit = baseDistribution.DoseUnit;
            var accumulated = new double[count];

            for (int i = 0; i < count; i++)
                accumulated[i] = baseDistribution.VoxelDoses[i].Dose.Value;

            for (int p = 1; p < _plans.Count; p++)
            {
                var current = _plans[p].CalculateEqd0DoseDistribution(structureId, abRatio);
                if (current.VoxelDoses.Count != count || current.DoseUnit != doseUnit)
                    throw new InvalidOperationException("Plan sum dose grids are not compatible.");

                for (int i = 0; i < count; i++)
                    accumulated[i] += current.VoxelDoses[i].Dose.Value;
            }

            var points = new List<DoseCloudPoint<EQD0Value>>(count);
            for (int i = 0; i < count; i++)
                points.Add(new DoseCloudPoint<EQD0Value>(EQD0Value.New(accumulated[i], doseUnit), voxelVolume));

            return new DoseCloudPoints<EQD0Value>(points);
        }

        public IEnumerable<IPlanItem> GetChildPlans() => _plans;

        public void AddPlan(IPlanItem plan)
        {
            if (!CanAddPlan(plan))
                throw new InvalidOperationException("Plan cannot be added to the plan sum.");

            _plans.Add(plan);
        }

        public bool CanAddPlan(IPlanItem plan)
        {
            if (plan == null)
                return false;

            if (!string.IsNullOrWhiteSpace(PatientId) &&
                !string.Equals(PatientId, plan.PatientId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return _plans.All(existing =>
                !string.Equals(existing.PlanId, plan.PlanId, StringComparison.OrdinalIgnoreCase));
        }

        internal void SetPatientId(string patientId)
        {
            if (!string.IsNullOrWhiteSpace(patientId))
                _patientId = patientId;
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
