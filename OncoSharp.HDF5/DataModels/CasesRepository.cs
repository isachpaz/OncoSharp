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
    // Root container representing everything loaded from an HDF5 file.
    public sealed class CasesRepository
    {
        private readonly Dictionary<string, Patient> _patients =
            new Dictionary<string, Patient>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<Patient> Patients => _patients.Values;

        public IReadOnlyList<string> GetPatientIds()
        {
            if (_patients.Count == 0)
                return Array.Empty<string>();

            return _patients.Keys
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool TryGetPatient(string patientId, out Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patientId))
            {
                patient = null;
                return false;
            }

            return _patients.TryGetValue(patientId, out patient);
        }

        public void AddOrReplacePatient(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));
            _patients[patient.PatientId] = patient;
        }

        public Patient GetOrAddPatient(string patientId)
        {
            Patient patient;
            if (!_patients.TryGetValue(patientId, out patient))
            {
                patient = new Patient(patientId);
                _patients[patientId] = patient;
            }

            return patient;
        }

        public string GetPlanSumName(string patientId, string courseId) =>
            GetRequiredPatient(patientId).GetPlanSumName(courseId);

        public IReadOnlyList<string> GetPlanSumList(string patientId, string courseId) =>
            GetRequiredPatient(patientId).GetPlanSumList(courseId);

        public IReadOnlyList<string> GetStructuresNames(string patientId) =>
            GetRequiredPatient(patientId).GetStructuresNames();

        public DoseRef GetDoseData(string patientId, string courseId, string planId, string roiName) =>
            GetRequiredPatient(patientId).GetDoseData(courseId, planId, roiName);

        private Patient GetRequiredPatient(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("PatientId must be provided.", nameof(patientId));

            Patient patient;
            if (!_patients.TryGetValue(patientId, out patient))
                throw new KeyNotFoundException($"Patient '{patientId}' not found in repository.");

            return patient;
        }
    }

    
}
