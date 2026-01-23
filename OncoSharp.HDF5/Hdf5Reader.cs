// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.HDF5.DataModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using HDF.PInvoke;

namespace OncoSharp.HDF5
{
    public sealed class Hdf5Reader : IDisposable
    {
        private readonly string _filePath;
        private readonly long _fileId;
        private bool _disposed;

        public Hdf5Reader(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

            _filePath = Path.GetFullPath(filePath);
            _fileId = H5F.open(_filePath, H5F.ACC_RDONLY);
            if (_fileId < 0)
                throw new IOException($"Failed to open HDF5 file: {_filePath}");
        }

        public static Hdf5Reader Open(string filePath) => new Hdf5Reader(filePath);

        public void Dispose()
        {
            if (_disposed) return;
            if (_fileId >= 0) H5F.close(_fileId);
            _disposed = true;
        }

        public string GetPlanSumName(string patientId, string courseId)
        {
            var list = GetPlanSumList(patientId, courseId);
            return list.FirstOrDefault();
        }

        public IReadOnlyList<string> GetPlanSumList(string patientId, string courseId)
        {
            EnsureNotDisposed();
            var planSumGroup = $"{BuildCoursePath(patientId, courseId)}/plan_sums";
            return GetChildNames(planSumGroup);
        }

        /// <summary>
        /// Returns all patient identifiers under /patients.
        /// </summary>
        public IReadOnlyList<string> GetPatientIds()
        {
            EnsureNotDisposed();
            return GetChildNames("/patients");
        }

        public IReadOnlyList<string> GetStructuresNames(string patientId)
        {
            EnsureNotDisposed();
            var patientPath = BuildPatientPath(patientId);
            long groupId = H5G.open(_fileId, patientPath);
            if (groupId < 0) return Array.Empty<string>();

            try
            {
                return ReadStringArrayAttr(groupId, "ROINames");
            }
            finally
            {
                H5G.close(groupId);
            }
        }

        public double[] GetDoseData(string patientId, string courseId, string planId, string structureId)
        {
            EnsureNotDisposed();
            var datasetPath = BuildDosePath(patientId, courseId, planId, structureId);
            if (!LinkExists(_fileId, datasetPath))
                throw new IOException($"Dose dataset not found: {datasetPath}");
            long datasetId = H5D.open(_fileId, datasetPath);
            if (datasetId < 0)
                throw new IOException($"Dose dataset not found: {datasetPath}");

            long spaceId = -1;
            try
            {
                spaceId = H5D.get_space(datasetId);
                int rank = H5S.get_simple_extent_ndims(spaceId);
                if (rank != 1)
                    throw new IOException("Dose dataset is expected to be one-dimensional.");

                ulong[] dims = new ulong[rank];
                H5S.get_simple_extent_dims(spaceId, dims, null);
                long length = (long)dims[0];

                return ReadDoseValues(datasetId, length, datasetPath);
            }
            finally
            {
                if (spaceId >= 0) H5S.close(spaceId);
                H5D.close(datasetId);
            }
        }

        /// <remarks>
        /// Example:
        /// <code>
        /// var repository = new RtRepository();
        /// using (var reader = new Hdf5Reader("matto_generated.h5"))
        /// {
        ///     var planSumName = reader.GetPlanSumName(spec1.Piz, spec1.CourseId);
        ///     var summationPlans = reader.GetPlanSumList(spec1.Piz, spec1.CourseId);
        ///     var structures = reader.GetStructuresNames(spec1.Piz);
        ///     var brainDose = reader.GetDoseData(spec1.Piz, spec1.CourseId, spec1.PlanId, "Brain");
        ///
        ///     var patientModel = reader.GetPatientModel(spec1.Piz);
        ///     Console.WriteLine($"First Plan Sum: {planSumName}");
        ///     Console.WriteLine($"Plan Sum Components: {string.Join(",", summationPlans)}");
        ///     Console.WriteLine($"Structures Count: {structures.Count}");
        ///     Console.WriteLine($"Brain voxels: {brainDose.Length}");
        ///
        ///     repository.Patients[patientModel.PatientId] = patientModel;
        ///     Console.WriteLine($"Repository now tracks {repository.Patients.Count} patient(s).");
        /// }
        /// </code>
        /// </remarks>
        public Patient GetPatientModel(string patientId)
        {
            if (patientId == "35830111")
            {
                Debug.WriteLine("....");
            }
            EnsureNotDisposed();
            var patient = new Patient(patientId);
            var patientPath = BuildPatientPath(patientId);
            var courseIds = GetChildNames($"{patientPath}/courses");
            foreach (var courseId in courseIds)
            {
                var course = patient.GetOrAddCourse(courseId);
                PopulatePlans(course, patientId, courseId);
                PopulatePlanSums(course, patientId, courseId);
            }

            return patient;
        }

        private void PopulatePlans(Course course, string patientId, string courseId)
        {
            var coursePath = BuildCoursePath(patientId, courseId);
            var planIds = GetChildNames($"{coursePath}/plans");
            foreach (var planId in planIds)
            {
                var plan = course.GetOrAddPlan(planId);
                PopulateRois(plan, patientId, courseId, planId);
            }
        }

        private void PopulateRois(Plan plan, string patientId, string courseId, string planId)
        {
            var planRoiPath = $"{BuildPlanPath(patientId, courseId, planId)}/rois";
            foreach (var roiId in GetChildNames(planRoiPath))
            {
                var datasetPath = $"{planRoiPath}/{roiId}/dose";
                if (!LinkExists(_fileId, datasetPath))
                    continue;
                long datasetId = H5D.open(_fileId, datasetPath);
                if (datasetId < 0) continue;

                long spaceId = -1;
                try
                {
                    spaceId = H5D.get_space(datasetId);
                    ulong[] dims = new ulong[1];
                    H5S.get_simple_extent_dims(spaceId, dims, null);

                    var roi = plan.GetOrAddRoi(roiId);
                    var elementCount = (long)dims[0];
                    var doseRef = new DoseRef(datasetPath, elementCount);
                    doseRef.SetValueFactory(() => ReadDoseValuesFromFile(datasetPath, elementCount));
                    roi.Dose = doseRef;

                    var voxelVolume = ReadDoubleAttr(datasetId, "VoxelVolume");
                    if (voxelVolume.HasValue)
                        roi.Dose.VoxelVolume = voxelVolume.Value;

                    var volumeUnits = ReadStringAttr(datasetId, "VolumeUnits");
                    if (!string.IsNullOrWhiteSpace(volumeUnits))
                        roi.Dose.VolumeUnits = volumeUnits;

                    var doseUnits = ReadStringAttr(datasetId, "DoseUnits");
                    if (!string.IsNullOrWhiteSpace(doseUnits))
                        roi.Dose.Units = doseUnits;

                    var fractions = ReadDoubleAttr(datasetId, "FractionsNumber");
                    if (fractions.HasValue && !plan.Attributes.ContainsKey("FractionsNumber"))
                    {
                        plan.Attributes["FractionsNumber"] =
                            fractions.Value.ToString(CultureInfo.InvariantCulture);
                    }
                }
                finally
                {
                    if (spaceId >= 0) H5S.close(spaceId);
                    H5D.close(datasetId);
                }
            }
        }

        private void PopulatePlanSums(Course course, string patientId, string courseId)
        {
            var planSumIds = GetPlanSumList(patientId, courseId);
            foreach (var planSumId in planSumIds)
            {
                PopulatePlanSum(course, patientId, courseId, planSumId);
            }
        }

        private void PopulatePlanSum(Course course, string patientId, string courseId, string planSumId)
        {
            var planSumPath = BuildPlanSumPath(patientId, courseId, planSumId);
            long groupId = H5G.open(_fileId, planSumPath);
            if (groupId < 0) throw new IOException($"Plan sum group not found: {planSumPath}");

            try
            {
                var ids = ReadStringArrayAttr(groupId, "ComponentPlanIds");
                var paths = ReadStringArrayAttr(groupId, "ComponentPlanPaths");
                var weights = ReadDoubleArrayAttr(groupId, "Weights");
                var planSum = course.GetOrAddPlanSum(planSumId);
                planSum.Attributes["ComponentPlanIds"] = string.Join(",", ids);
                planSum.Attributes["ComponentPlanPaths"] = string.Join(",", paths);
                planSum.Attributes["Weights"] = string.Join(",",
                    weights.Select(w => w.ToString(CultureInfo.InvariantCulture)));
                planSum.Attributes["Hdf5Path"] = planSumPath;
            }
            finally
            {
                H5G.close(groupId);
            }
        }

        private IReadOnlyList<string> GetChildNames(string groupPath)
        {
            if (H5L.exists(_fileId, groupPath, H5P.DEFAULT) <= 0)
                return Array.Empty<string>();

            long groupId = H5G.open(_fileId, groupPath);
            if (groupId < 0) return Array.Empty<string>();

            try
            {
                var names = new List<string>();
                H5G.info_t info = new H5G.info_t();
                if (H5G.get_info(groupId, ref info) < 0)
                    return names;

                for (ulong i = 0; i < info.nlinks; i++)
                {
                    var sb = new StringBuilder(256);
                    long size = H5L.get_name_by_idx(groupId, ".", H5.index_t.NAME, H5.iter_order_t.NATIVE, i, sb, new IntPtr(sb.Capacity), H5P.DEFAULT).ToInt64();
                    if (size < 0) continue;
                    if (size >= sb.Capacity - 1)
                    {
                        sb = new StringBuilder((int)size + 1);
                        size = H5L.get_name_by_idx(groupId, ".", H5.index_t.NAME, H5.iter_order_t.NATIVE, i, sb, new IntPtr(sb.Capacity), H5P.DEFAULT).ToInt64();
                        if (size < 0) continue;
                    }

                    var name = sb.ToString();
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name.Trim());
                }

                return names;
            }
            finally
            {
                H5G.close(groupId);
            }
        }

        private static string BuildPatientPath(string patientId)
        {
            var patientSegment = (patientId ?? string.Empty).Trim('/');
            if (string.IsNullOrEmpty(patientSegment))
                throw new ArgumentException("PatientId is required.");
            return $"/patients/{patientSegment}";
        }

        private static string BuildCoursePath(string patientId, string courseId)
        {
            var courseSegment = (courseId ?? string.Empty).Trim('/');
            if (string.IsNullOrEmpty(courseSegment))
                throw new ArgumentException("CourseId is required.");
            return $"{BuildPatientPath(patientId)}/courses/{courseSegment}";
        }

        private static string BuildPlanPath(string patientId, string courseId, string planId)
        {
            var planSegment = (planId ?? string.Empty).Trim('/');
            if (string.IsNullOrEmpty(planSegment))
                throw new ArgumentException("PlanId is required.");
            return $"{BuildCoursePath(patientId, courseId)}/plans/{planSegment}";
        }

        private static string BuildDosePath(string patientId, string courseId, string planId, string structureId)
        {
            var structureSegment = (structureId ?? string.Empty).Trim('/');
            if (string.IsNullOrEmpty(structureSegment))
                throw new ArgumentException("StructureId is required.");
            return $"{BuildPlanPath(patientId, courseId, planId)}/rois/{structureSegment}/dose";
        }

        private static string BuildPlanSumPath(string patientId, string courseId, string planSumId)
        {
            var planSumSegment = (planSumId ?? string.Empty).Trim('/');
            if (string.IsNullOrEmpty(planSumSegment))
                throw new ArgumentException("PlanSumId is required.");
            return $"{BuildCoursePath(patientId, courseId)}/plan_sums/{planSumSegment}";
        }

        private static IReadOnlyList<string> ReadStringArrayAttr(long objId, string name)
        {
            var result = new List<string>();
            if (H5A.exists(objId, name) <= 0) return result;

            long attrId = -1;
            long typeId = -1;
            long spaceId = -1;
            GCHandle handle = default;

            try
            {
                attrId = H5A.open(objId, name);
                if (attrId < 0) return result;

                typeId = H5A.get_type(attrId);
                spaceId = H5A.get_space(attrId);

                int rank = H5S.get_simple_extent_ndims(spaceId);
                if (rank != 1) return result;

                ulong[] dims = new ulong[rank];
                H5S.get_simple_extent_dims(spaceId, dims, null);
                ulong count = dims[0];
                if (count == 0) return result;

                var buffer = new IntPtr[count];
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                var status = H5A.read(attrId, typeId, handle.AddrOfPinnedObject());
                if (status < 0) return result;

                for (int i = 0; i < (int)count; i++)
                {
                    var value = PtrToStringUtf8(buffer[i]);
                    if (!string.IsNullOrWhiteSpace(value))
                        result.Add(value.Trim());
                    if (buffer[i] != IntPtr.Zero)
                        H5.free_memory(buffer[i]);
                }
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                if (typeId >= 0) H5T.close(typeId);
                if (spaceId >= 0) H5S.close(spaceId);
                if (attrId >= 0) H5A.close(attrId);
            }

            return result;
        }

        private double[] ReadDoseValuesFromFile(string datasetPath, long length)
        {
            long fileId = -1;
            long datasetId = -1;
            long spaceId = -1;

            try
            {
                fileId = H5F.open(_filePath, H5F.ACC_RDONLY);
                if (fileId < 0)
                    throw new IOException($"Failed to reopen HDF5 file: {_filePath}");

                if (!LinkExists(fileId, datasetPath))
                    throw new IOException($"Dose dataset not found: {datasetPath}");

                datasetId = H5D.open(fileId, datasetPath);
                if (datasetId < 0)
                    throw new IOException($"Dose dataset not found: {datasetPath}");

                spaceId = H5D.get_space(datasetId);
                if (length <= 0)
                {
                    ulong[] dims = new ulong[1];
                    H5S.get_simple_extent_dims(spaceId, dims, null);
                    length = (long)dims[0];
                }

                return ReadDoseValues(datasetId, length, datasetPath);
            }
            finally
            {
                if (spaceId >= 0) H5S.close(spaceId);
                if (datasetId >= 0) H5D.close(datasetId);
                if (fileId >= 0) H5F.close(fileId);
            }
        }

        private static double[] ReadDoseValues(long datasetId, long length, string datasetPath)
        {
            if (length <= 0) return Array.Empty<double>();

            var buffer = new double[length];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var status = H5D.read(datasetId, H5T.NATIVE_DOUBLE, H5S.ALL, H5S.ALL, H5P.DEFAULT,
                    handle.AddrOfPinnedObject());
                if (status < 0)
                    throw new IOException($"Failed to read dose dataset: {datasetPath}");
            }
            finally
            {
                handle.Free();
            }

            return buffer;
        }

        private static IReadOnlyList<double> ReadDoubleArrayAttr(long objId, string name)
        {
            var result = new List<double>();
            if (H5A.exists(objId, name) <= 0) return result;

            long attrId = -1;
            long spaceId = -1;
            GCHandle handle = default;

            try
            {
                attrId = H5A.open(objId, name);
                if (attrId < 0) return result;

                spaceId = H5A.get_space(attrId);
                int rank = H5S.get_simple_extent_ndims(spaceId);
                if (rank != 1) return result;

                ulong[] dims = new ulong[rank];
                H5S.get_simple_extent_dims(spaceId, dims, null);
                ulong count = dims[0];
                if (count == 0) return result;

                var buffer = new double[count];
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var status = H5A.read(attrId, H5T.NATIVE_DOUBLE, handle.AddrOfPinnedObject());
                if (status < 0) return result;

                result.AddRange(buffer);
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                if (spaceId >= 0) H5S.close(spaceId);
                if (attrId >= 0) H5A.close(attrId);
            }

            return result;
        }

        private static double? ReadDoubleAttr(long objId, string name)
        {
            if (H5A.exists(objId, name) <= 0) return null;

            long attrId = -1;
            GCHandle handle = default;

            try
            {
                attrId = H5A.open(objId, name);
                if (attrId < 0) return null;

                var buffer = new double[1];
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var status = H5A.read(attrId, H5T.NATIVE_DOUBLE, handle.AddrOfPinnedObject());
                if (status < 0) return null;

                return buffer[0];
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                if (attrId >= 0) H5A.close(attrId);
            }
        }

        private static string ReadStringAttr(long objId, string name)
        {
            if (H5A.exists(objId, name) <= 0) return null;

            long attrId = -1;
            long typeId = -1;
            GCHandle handle = default;
            IntPtr[] ptr = null;

            try
            {
                attrId = H5A.open(objId, name);
                if (attrId < 0) return null;

                typeId = H5A.get_type(attrId);
                ptr = new IntPtr[1];
                handle = GCHandle.Alloc(ptr, GCHandleType.Pinned);
                var status = H5A.read(attrId, typeId, handle.AddrOfPinnedObject());
                if (status < 0) return null;

                var value = PtrToStringUtf8(ptr[0]);
                return string.IsNullOrEmpty(value) ? null : value.Trim();
            }
            finally
            {
                if (ptr != null && ptr[0] != IntPtr.Zero)
                    H5.free_memory(ptr[0]);
                if (handle.IsAllocated) handle.Free();
                if (typeId >= 0) H5T.close(typeId);
                if (attrId >= 0) H5A.close(attrId);
            }
        }

        private static string PtrToStringUtf8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return string.Empty;

            int length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
            {
                length++;
            }

            if (length == 0) return string.Empty;

            var buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, length);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        private static bool LinkExists(long fileId, string path)
        {
            if (fileId < 0 || string.IsNullOrWhiteSpace(path))
                return false;

            return H5L.exists(fileId, path, H5P.DEFAULT) > 0;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Hdf5Reader));
        }
    }
}
