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
    public sealed class Patient : IPatientModel
    {
        public string PatientId { get; }

        private readonly Dictionary<string, Course> _courses =
            new Dictionary<string, Course>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<Course> Courses => _courses.Values;

        public Patient(string patientId)
        {
            PatientId = patientId;
        }

        public Course GetOrAddCourse(string courseId)
        {
            Course course;
            if (!_courses.TryGetValue(courseId, out course))
            {
                course = new Course(courseId, PatientId);
                _courses[courseId] = course;
            }

            return course;
        }

        public IReadOnlyList<string> GetCourseIds()
        {
            if (_courses.Count == 0)
                return Array.Empty<string>();

            return _courses.Keys
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<string> GetPlanIds(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
                return Array.Empty<string>();

            Course course;
            if (!_courses.TryGetValue(courseId, out course))
                return Array.Empty<string>();

            return course.GetPlanIds();
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
            if (!_courses.TryGetValue(courseId, out course))
                return Array.Empty<string>();

            return course.GetPlanSumIds();
        }

        public IReadOnlyList<string> GetStructuresNames()
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var course in Courses)
            {
                foreach (var plan in course.Plans)
                {
                    foreach (var roi in plan.Rois)
                    {
                        if (seen.Add(roi.Name))
                            result.Add(roi.Name);
                    }
                }
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public DoseRef GetDoseData(string courseId, string planId, string roiName)
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

            if (roi.Dose == null)
                throw new InvalidOperationException($"Dose values are not available for ROI '{roiName}'.");

            return roi.Dose;
        }

        private Course RequireCourse(string courseId)
        {
            Course course;
            if (!_courses.TryGetValue(courseId, out course))
                throw new KeyNotFoundException($"Course '{courseId}' not found for patient '{PatientId}'.");
            return course;
        }

        private static Plan RequirePlan(Course course, string planId)
        {
            Plan plan;
            if (!course.TryGetPlan(planId, out plan))
                throw new KeyNotFoundException($"Plan '{planId}' not found in course '{course.CourseId}'.");
            return plan;
        }

        private static Roi RequireRoi(Plan plan, string roiName)
        {
            Roi roi;
            if (!plan.TryGetRoi(roiName, out roi))
                throw new KeyNotFoundException($"ROI '{roiName}' not found in plan '{plan.PlanId}'.");
            return roi;
        }

    }
}
