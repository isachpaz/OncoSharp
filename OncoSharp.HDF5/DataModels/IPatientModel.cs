// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;

namespace OncoSharp.HDF5.DataModels
{
    public interface IPatientModel
    {
        string PatientId { get; }
        IReadOnlyList<string> GetCourseIds();
        IReadOnlyList<string> GetPlanIds(string courseId);
        string GetPlanSumName(string courseId);
        IReadOnlyList<string> GetPlanSumList(string courseId);
        IReadOnlyList<string> GetStructuresNames();
        DoseRef GetDoseData(string courseId, string planId, string roiName);
    }
}