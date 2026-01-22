// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OncoSharp.HDF5.DataModels
{
   
    public interface IPatientModel
    {
        string GetPlanSumName(string patientId, string courseId);
        IReadOnlyList<string> GetPlanSumList(string patientId, string courseId);
        IReadOnlyList<string> GetStructuresNames(string patientId);
        DoseData GetDoseData(string patientId, string courseId, string planId, string roiName);
    }

    // Root container representing everything loaded from an HDF5 file.
    public sealed class CasesRepository : IPatientModel
    {
        public Dictionary<string, Patient> Patients { get; } =
            new Dictionary<string, Patient>(StringComparer.OrdinalIgnoreCase);

        public Patient GetOrAddPatient(string patientId)
        {
            Patient patient;
            if (!Patients.TryGetValue(patientId, out patient))
            {
                patient = new Patient(patientId);
                Patients[patientId] = patient;
            }

            return patient;
        }

        public string GetPlanSumName(string patientId, string courseId) =>
            GetRequiredPatient(patientId).GetPlanSumName(courseId);

        public IReadOnlyList<string> GetPlanSumList(string patientId, string courseId) =>
            GetRequiredPatient(patientId).GetPlanSumList(courseId);

        public IReadOnlyList<string> GetStructuresNames(string patientId) =>
            GetRequiredPatient(patientId).GetStructuresNames();

        public DoseData GetDoseData(string patientId, string courseId, string planId, string roiName) =>
            GetRequiredPatient(patientId).GetDoseData(courseId, planId, roiName);

        private Patient GetRequiredPatient(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("PatientId must be provided.", nameof(patientId));

            Patient patient;
            if (!Patients.TryGetValue(patientId, out patient))
                throw new KeyNotFoundException($"Patient '{patientId}' not found in repository.");

            return patient;
        }
    }

    // /patients/<patient>
    public sealed class Patient : IPatientModel
    {
        public string PatientId { get; }

        public Dictionary<string, Course> Courses { get; } =
            new Dictionary<string, Course>(StringComparer.OrdinalIgnoreCase);

        public Patient(string patientId)
        {
            PatientId = patientId;
        }

        public Course GetOrAddCourse(string courseId)
        {
            Course course;
            if (!Courses.TryGetValue(courseId, out course))
            {
                course = new Course(courseId);
                Courses[courseId] = course;
            }

            return course;
        }

        public string GetPlanSumName(string courseId)
        {
            var planSums = GetPlanSumList(courseId);
            return planSums.FirstOrDefault();
        }

        public IReadOnlyList<string> GetPlanSumList(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
                return Array.Empty<string>();

            Course course;
            if (!Courses.TryGetValue(courseId, out course))
                return Array.Empty<string>();

            if (course.PlanSums.Count == 0)
                return Array.Empty<string>();

            return course.PlanSums.Keys
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<string> GetStructuresNames()
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var course in Courses.Values)
            {
                foreach (var plan in course.Plans.Values)
                {
                    foreach (var roi in plan.Rois.Keys)
                    {
                        if (seen.Add(roi))
                            result.Add(roi);
                    }
                }
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public DoseData GetDoseData(string courseId, string planId, string roiName)
        {
            if (string.IsNullOrWhiteSpace(courseId) ||
                string.IsNullOrWhiteSpace(planId) ||
                string.IsNullOrWhiteSpace(roiName))
            {
                throw new ArgumentException("CourseId, PlanId, and RoiName must be provided.");
            }

            var course = RequireCourse(courseId);
            var plan = RequirePlan(course, planId);
            var roi = RequireRoi(plan, roiName);

            if (roi.Dose == null || roi.Dose.Values == null)
                throw new InvalidOperationException($"Dose values are not available for ROI '{roiName}'.");

            var fractions = ParseDouble(plan.Attributes, "FractionsNumber");

            return DoseData.FromDoseRef(
                PatientId,
                course.CourseId,
                plan.PlanId,
                roi.Name,
                roi.Dose,
                fractions);
        }

        string IPatientModel.GetPlanSumName(string patientId, string courseId)
        {
            EnsurePatient(patientId);
            return GetPlanSumName(courseId);
        }

        IReadOnlyList<string> IPatientModel.GetPlanSumList(string patientId, string courseId)
        {
            EnsurePatient(patientId);
            return GetPlanSumList(courseId);
        }

        IReadOnlyList<string> IPatientModel.GetStructuresNames(string patientId)
        {
            EnsurePatient(patientId);
            return GetStructuresNames();
        }

        DoseData IPatientModel.GetDoseData(string patientId, string courseId, string planId, string roiName)
        {
            EnsurePatient(patientId);
            return GetDoseData(courseId, planId, roiName);
        }

        private void EnsurePatient(string patientId)
        {
            if (!string.Equals(PatientId, patientId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Requested patient '{patientId}' does not match loaded '{PatientId}'.");
        }

        private Course RequireCourse(string courseId)
        {
            Course course;
            if (!Courses.TryGetValue(courseId, out course))
                throw new KeyNotFoundException($"Course '{courseId}' not found for patient '{PatientId}'.");
            return course;
        }

        private static Plan RequirePlan(Course course, string planId)
        {
            Plan plan;
            if (!course.Plans.TryGetValue(planId, out plan))
                throw new KeyNotFoundException($"Plan '{planId}' not found in course '{course.CourseId}'.");
            return plan;
        }

        private static Roi RequireRoi(Plan plan, string roiName)
        {
            Roi roi;
            if (!plan.Rois.TryGetValue(roiName, out roi))
                throw new KeyNotFoundException($"ROI '{roiName}' not found in plan '{plan.PlanId}'.");
            return roi;
        }

        private static double? ParseDouble(Dictionary<string, string> source, string key)
        {
            if (source == null || !source.TryGetValue(key, out var value))
                return null;

            if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }
    }

    // /patients/<patient>/courses/<course>
    public sealed class Course
    {
        public string CourseId { get; }

        // /plans/<plan>
        public Dictionary<string, Plan> Plans { get; } =
            new Dictionary<string, Plan>(StringComparer.OrdinalIgnoreCase);

        // /plan_sums/<planSum>
        public Dictionary<string, PlanSum> PlanSums { get; } =
            new Dictionary<string, PlanSum>(StringComparer.OrdinalIgnoreCase);

        public Course(string courseId)
        {
            CourseId = courseId;
        }

        public Plan GetOrAddPlan(string planId)
        {
            Plan plan;
            if (!Plans.TryGetValue(planId, out plan))
            {
                plan = new Plan(planId);
                Plans[planId] = plan;
            }

            return plan;
        }

        public PlanSum GetOrAddPlanSum(string planSumId)
        {
            PlanSum planSum;
            if (!PlanSums.TryGetValue(planSumId, out planSum))
            {
                planSum = new PlanSum(planSumId);
                PlanSums[planSumId] = planSum;
            }

            return planSum;
        }
    }

    // /plans/<plan>
    public sealed class Plan
    {
        public string PlanId { get; }

        // /rois/<roi>
        public Dictionary<string, Roi> Rois { get; } =
            new Dictionary<string, Roi>(StringComparer.OrdinalIgnoreCase);

        // Store plan-level metadata (fractions, techniques, etc.)
        public Dictionary<string, string> Attributes { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Plan(string planId)
        {
            PlanId = planId;
        }

        public Roi GetOrAddRoi(string roiName)
        {
            Roi roi;
            if (!Rois.TryGetValue(roiName, out roi))
            {
                roi = new Roi(roiName);
                Rois[roiName] = roi;
            }

            return roi;
        }
    }

    // /plan_sums/<planSum>
    public sealed class PlanSum
    {
        public string PlanSumId { get; }

        // Store arbitrary attributes (component ids, weights, HDF5 paths, etc.)
        public Dictionary<string, string> Attributes { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public PlanSum(string planSumId)
        {
            PlanSumId = planSumId;
        }
    }

    // /rois/<roi>
    public sealed class Roi
    {
        public string Name { get; }

        // Reference to the dose dataset stored under /rois/<roi>/dose
        public DoseRef Dose { get; set; }

        public Dictionary<string, string> Attributes { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Roi(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Reference to a dose dataset's location plus metadata that can be populated on demand.
    /// </summary>
    public sealed class DoseRef
    {
        public string DatasetPath { get; }
        public long ElementCount { get; }

        // Optionally set when discovering dataspace shape or datatype.
        public int[] Shape { get; set; }
        public string DType { get; set; }
        public string Units { get; set; }
        public double? VoxelVolume { get; set; }
        public string VolumeUnits { get; set; }

        private Func<double[]> _valueFactory;
        private double[] _values;
        private readonly object _sync = new object();

        public double[] Values
        {
            get
            {
                if (_values != null) return _values;
                lock (_sync)
                {
                    if (_values == null)
                    {
                        _values = (_valueFactory?.Invoke()) ?? Array.Empty<double>();
                        _valueFactory = null;
                    }
                }
                return _values;
            }
            set
            {
                lock (_sync)
                {
                    _values = value ?? Array.Empty<double>();
                    _valueFactory = null;
                }
            }
        }

        public DoseRef(string datasetPath, long elementCount)
        {
            DatasetPath = datasetPath;
            ElementCount = elementCount;
            _values = Array.Empty<double>();
        }

        public void SetValueFactory(Func<double[]> valueFactory)
        {
            lock (_sync)
            {
                _valueFactory = valueFactory;
                _values = null;
            }
        }

        public override string ToString()
        {
            return DatasetPath + " (N=" + ElementCount + ")";
        }
    }

    public sealed class DoseData
    {
        public string PatientId { get; }
        public string CourseId { get; }
        public string PlanId { get; }
        public string RoiName { get; }
        public string DatasetPath { get; }
        public double[] Values { get; }
        public string DoseUnits { get; }
        public double? VoxelVolume { get; }
        public string VolumeUnits { get; }
        public double? FractionsNumber { get; }

        private DoseData(
            string patientId,
            string courseId,
            string planId,
            string roiName,
            string datasetPath,
            double[] values,
            string doseUnits,
            double? voxelVolume,
            string volumeUnits,
            double? fractionsNumber)
        {
            PatientId = patientId;
            CourseId = courseId;
            PlanId = planId;
            RoiName = roiName;
            DatasetPath = datasetPath;
            Values = values ?? Array.Empty<double>();
            DoseUnits = doseUnits;
            VoxelVolume = voxelVolume;
            VolumeUnits = volumeUnits;
            FractionsNumber = fractionsNumber;
        }

        internal static DoseData FromDoseRef(
            string patientId,
            string courseId,
            string planId,
            string roiName,
            DoseRef doseRef,
            double? fractionsNumber)
        {
            if (doseRef == null)
                throw new ArgumentNullException(nameof(doseRef));

            return new DoseData(
                patientId,
                courseId,
                planId,
                roiName,
                doseRef.DatasetPath,
                doseRef.Values ?? Array.Empty<double>(),
                doseRef.Units,
                doseRef.VoxelVolume,
                doseRef.VolumeUnits,
                fractionsNumber);
        }
    }
}