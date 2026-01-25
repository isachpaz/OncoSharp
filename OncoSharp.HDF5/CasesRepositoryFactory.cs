// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using OncoSharp.HDF5.DataModels;

namespace OncoSharp.HDF5
{
   /// <summary>
    /// Helper methods for loading <see cref="RtRepository" /> instances directly from HDF5 files.
    /// </summary>
    public static class CasesRepositoryFactory
    {
        /// <summary>
        /// Creates a new repository and populates it with every patient found in the specified HDF5 file.
        /// </summary>
        public static CasesRepository LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided.", nameof(filePath));

            var repository = new CasesRepository();
            PopulateRepository(repository, filePath);
            return repository;
        }

        /// <summary>
        /// Populates an existing repository with patients read from the specified HDF5 file.
        /// Existing entries with the same patient identifiers are replaced.
        /// </summary>
        public static void PopulateRepository(CasesRepository repository, string filePath)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must be provided.", nameof(filePath));

            using (var reader = new Hdf5Reader(filePath))
            {
                foreach (var patientId in reader.GetPatientIds())
                {
                    if (string.IsNullOrWhiteSpace(patientId))
                        continue;

                    var patient = reader.GetPatientModel(patientId);
                    repository.AddOrReplacePatient(patient);
                }
            }
        }
    }
}
