// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace OncoSharp.HDF5.DataModels
{
    public sealed class Course
    {
        public string CourseId { get; }
        public string PatientId { get; }

        // /plans/<plan>
        private readonly Dictionary<string, Plan> _plans =
            new Dictionary<string, Plan>(StringComparer.OrdinalIgnoreCase);

        // /plan_sums/<planSum>
        private readonly Dictionary<string, PlanSum> _planSums =
            new Dictionary<string, PlanSum>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<Plan> Plans => _plans.Values;
        public IReadOnlyCollection<PlanSum> PlanSums => _planSums.Values;

        public Course(string courseId)
            : this(courseId, null)
        {
        }

        public Course(string courseId, string patientId)
        {
            CourseId = courseId;
            PatientId = patientId;
        }

        public IReadOnlyList<string> GetPlanIds()
        {
            if (_plans.Count == 0)
                return Array.Empty<string>();

            return _plans.Keys
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<string> GetPlanSumIds()
        {
            if (_planSums.Count == 0)
                return Array.Empty<string>();

            return _planSums.Keys
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool TryGetPlan(string planId, out Plan plan) =>
            _plans.TryGetValue(planId, out plan);

        public bool TryGetPlanSum(string planSumId, out PlanSum planSum) =>
            _planSums.TryGetValue(planSumId, out planSum);

        public Plan GetOrAddPlan(string planId)
        {
            Plan plan;
            if (!_plans.TryGetValue(planId, out plan))
            {
                plan = new Plan(planId, PatientId);
                _plans[planId] = plan;
            }
            else if (plan.PatientId == null)
            {
                plan.PatientId = PatientId;
            }

            return plan;
        }

        public PlanSum GetOrAddPlanSum(string planSumId)
        {
            PlanSum planSum;
            if (!_planSums.TryGetValue(planSumId, out planSum))
            {
                planSum = new PlanSum(planSumId, PatientId);
                _planSums[planSumId] = planSum;
            }
            else
            {
                planSum.SetPatientId(PatientId);
            }

            return planSum;
        }
    }
}
