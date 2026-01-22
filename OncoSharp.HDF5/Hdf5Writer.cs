// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using HDF.PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace OncoSharp.HDF5
{
    public sealed class DoseDatasetSpec
    {
        public string DosePath { get; set; }
        // Full absolute dataset path ending in "/dose"
        // 1D dose values (float64)
        public double[] Values { get; set; }

        // Dataset attributes (as per your HDF5 schema)
        public double VoxelVolume { get; set; }
        public string VolumeUnits { get; set; }
        public string DoseUnits { get; set; }

        public double FractionsNumber { get; set; }
        public string Piz { get; set; }         // patient id in your attrs
        public string CourseId { get; set; }
        public string PlanId { get; set; }
        public string ROIName { get; set; }
        public string SourceFile { get; set; }
    }

    public sealed class PlanSumSpec
    {
        public string PlanSumId { get; set; }
        public string PatientId { get; set; }
        public string CourseId { get; set; }
        public IEnumerable<string> ComponentPlanIds { get; set; }
        public IEnumerable<DoseDatasetSpec> ComponentPlans { get; set; }
        public IEnumerable<double> Weights { get; set; }
    }

    public static class Hdf5Writer
    {
        /// <summary>
        /// Creates a new HDF5 file and writes all dose datasets + their attributes.
        /// </summary>
        public static void WriteNewFile(string outputPath, IEnumerable<DoseDatasetSpec> items)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or whitespace.", nameof(outputPath));
            if (items == null) throw new ArgumentNullException(nameof(items));

            var specs = items as IList<DoseDatasetSpec> ?? new List<DoseDatasetSpec>(items);
            var metadataAggregator = new PatientMetadataAggregator();

            long fileId = H5F.create(outputPath, H5F.ACC_TRUNC);
            if (fileId < 0)
                throw new IOException($"Failed to create HDF5 file: {outputPath}");

            try
            {
                foreach (var spec in specs)
                {
                    metadataAggregator.AddDose(spec);
                    AppendDoseDatasetNative(fileId, spec);
                }

                WritePatientMetadata(fileId, metadataAggregator);
            }
            finally
            {
                if (fileId >= 0) H5F.close(fileId);
            }
        }

        private static void WritePatientMetadata(long fileId, PatientMetadataAggregator aggregator)
        {
            if (aggregator == null) return;

            foreach (var entry in aggregator.Items)
            {
                var patientId = entry.Key;
                if (string.IsNullOrWhiteSpace(patientId)) continue;

                var patientSegment = patientId.Trim('/');
                var patientPath = $"/patients/{patientSegment}";

                EnsureNativeGroups(fileId, patientPath);

                long groupId = H5G.open(fileId, patientPath);
                if (groupId < 0) throw new IOException($"Failed to open patient group: {patientPath}");

                try
                {
                    var metadata = entry.Value;

                    var courses = MergeStringLists(ReadNativeStringArrayAttr(groupId, "CourseNames"), metadata.CourseNames);
                    WriteNativeStringArrayAttr(groupId, "CourseNames", courses);

                    var plans = MergeStringLists(ReadNativeStringArrayAttr(groupId, "PlanNames"), metadata.PlanNames);
                    WriteNativeStringArrayAttr(groupId, "PlanNames", plans);

                    var rois = MergeStringLists(ReadNativeStringArrayAttr(groupId, "ROINames"), metadata.ROINames);
                    WriteNativeStringArrayAttr(groupId, "ROINames", rois);

                    var planSums = MergeStringLists(ReadNativeStringArrayAttr(groupId, "PlanSums"), metadata.PlanSumIds);
                    WriteNativeStringArrayAttr(groupId, "PlanSums", planSums);

                    WriteNativeStringAttr(groupId, "PatientId", patientSegment);
                }
                finally
                {
                    H5G.close(groupId);
                }
            }
        }

        /// <summary>
        /// Convenience overload to avoid manual array creation when adding a small number of datasets.
        /// </summary>
        public static void WriteNewFile(string outputPath, params DoseDatasetSpec[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            WriteNewFile(outputPath, (IEnumerable<DoseDatasetSpec>)items);
        }

        /// <summary>
        /// Appends one or more dose datasets into an existing HDF5 file. Creates the file if it does not exist.
        /// </summary>
        public static void AppendDoseDatasets(string filePath, IEnumerable<DoseDatasetSpec> items)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
            if (items == null) throw new ArgumentNullException(nameof(items));

            var specs = items as IList<DoseDatasetSpec> ?? new List<DoseDatasetSpec>(items);
            var metadataAggregator = new PatientMetadataAggregator();
            if (specs.Count == 0) return;

            if (!File.Exists(filePath))
            {
                WriteNewFile(filePath, specs);
                return;
            }

            long fileId = H5F.open(filePath, H5F.ACC_RDWR);
            if (fileId < 0)
                throw new IOException($"Failed to open HDF5 file for append: {filePath}");

            try
            {
                foreach (var spec in specs)
                {
                    metadataAggregator.AddDose(spec);
                    AppendDoseDatasetNative(fileId, spec);
                }

                WritePatientMetadata(fileId, metadataAggregator);
            }
            finally
            {
                if (fileId >= 0) H5F.close(fileId);
            }
        }

        /// <summary>
        /// Appends datasets (params overload).
        /// </summary>
        public static void AppendDoseDatasets(string filePath, params DoseDatasetSpec[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            AppendDoseDatasets(filePath, (IEnumerable<DoseDatasetSpec>)items);
        }

        public static FileBuilder CreateFileBuilder() => new FileBuilder();

        public sealed class FileBuilder
        {
            private readonly List<DoseDatasetSpec> _items = new List<DoseDatasetSpec>();
            private readonly List<PlanSumSpec> _planSums = new List<PlanSumSpec>();

            internal FileBuilder()
            {
            }

            public FileBuilder AddDoseDataset(DoseDatasetSpec item)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));
                _items.Add(item);
                return this;
            }

            public FileBuilder AddPlanSum(PlanSumSpec planSum)
            {
                if (planSum == null) throw new ArgumentNullException(nameof(planSum));
                _planSums.Add(planSum);
                return this;
            }

            public void Write(string outputPath)
            {
                WriteNewFile(outputPath, _items);
                if (_planSums.Count > 0)
                {
                    AppendPlanSums(outputPath, _planSums);
                }
            }
        }

        public static class SchemaVersioning
        {
            public static void WriteRootSchemaInfo(long fileId, string schemaVersion, object capabilities)
            {
                long root = H5G.open(fileId, "/");
                if (root < 0) throw new Exception("Failed to open root group.");

                try
                {
                    var version = schemaVersion ?? "0.0.0";
                    WriteNativeStringAttr(root, "SchemaName", "RTDataset");
                    WriteNativeStringAttr(root, "SchemaVersion", version);

                    int major = 0;
                    var parts = version.Split('.');
                    if (parts.Length > 0) int.TryParse(parts[0], out major);
                    WriteNativeStringAttr(root, "SchemaMajor", major.ToString());

                    WriteNativeStringAttr(root, "CreatedUtc", DateTime.UtcNow.ToString("o"));
                    WriteNativeStringAttr(root, "CreatedLocal", DateTime.Now.ToString("o"));

                    string capsJson = JsonSerializer.Serialize(capabilities ?? new { });
                    WriteNativeStringAttr(root, "SchemaCapabilitiesJson", capsJson);
                }
                finally
                {
                    H5G.close(root);
                }
            }
        }

        public static void AppendPlanSums(string filePath, IEnumerable<PlanSumSpec> planSums)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
            if (planSums == null) throw new ArgumentNullException(nameof(planSums));

            var specs = planSums as IList<PlanSumSpec> ?? new List<PlanSumSpec>(planSums);
            var metadataAggregator = new PatientMetadataAggregator();
            if (specs.Count == 0) return;

            long fileId = File.Exists(filePath) ? H5F.open(filePath, H5F.ACC_RDWR) : H5F.create(filePath, H5F.ACC_TRUNC);
            if (fileId < 0)
                throw new IOException($"Failed to open HDF5 file: {filePath}");

            try
            {
                foreach (var planSum in specs)
                {
                    metadataAggregator.AddPlanSum(planSum);
                    AppendPlanSumGroup(fileId, planSum);
                }

                WritePatientMetadata(fileId, metadataAggregator);
            }
            finally
            {
                if (fileId >= 0) H5F.close(fileId);
            }
        }

        public static void AppendPlanSums(string filePath, params PlanSumSpec[] planSums)
        {
            if (planSums == null) throw new ArgumentNullException(nameof(planSums));
            AppendPlanSums(filePath, (IEnumerable<PlanSumSpec>)planSums);
        }

        public static void WriteSchemaInfo(string filePath, string schemaVersion, object capabilities)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

            long fileId = File.Exists(filePath) ? H5F.open(filePath, H5F.ACC_RDWR) : H5F.create(filePath, H5F.ACC_TRUNC);
            if (fileId < 0)
                throw new IOException($"Failed to open HDF5 file: {filePath}");

            try
            {
                SchemaVersioning.WriteRootSchemaInfo(fileId, schemaVersion, capabilities);
            }
            finally
            {
                if (fileId >= 0) H5F.close(fileId);
            }
        }

        private static void AppendDoseDatasetNative(long fileId, DoseDatasetSpec item)
        {
            Validate(item);
            var dosePath = GetDosePath(item);

            long spaceId = -1;
            long dsetId = -1;
            long dcplId = -1;
            var parentPath = dosePath.Substring(0, dosePath.LastIndexOf('/'));

            EnsureNativeGroups(fileId, parentPath);

            ulong[] dims = { (ulong)item.Values.Length };
            spaceId = H5S.create_simple(rank: 1, dims: dims, maxdims: dims);
            if (spaceId < 0) throw new IOException("Failed to create dataspace.");

            // Configure compression (chunked + deflate)
            dcplId = CreateCompressedDatasetProperties(dims[0]);

            try
            {
                if (NativeLinkExists(fileId, dosePath))
                {
                    var status = H5L.delete(fileId, dosePath);
                    if (status < 0) throw new IOException("Failed to delete existing dataset before append.");
                }

                dsetId = H5D.create(fileId, dosePath, H5T.NATIVE_DOUBLE, spaceId, H5P.DEFAULT, dcplId, H5P.DEFAULT);
                if (dsetId < 0) throw new IOException("Failed to create dataset for append.");

                var handle = GCHandle.Alloc(item.Values, GCHandleType.Pinned);
                try
                {
                    var status = H5D.write(dsetId, H5T.NATIVE_DOUBLE, H5S.ALL, H5S.ALL, H5P.DEFAULT,
                        handle.AddrOfPinnedObject());
                    if (status < 0) throw new IOException("Failed to write dataset values.");
                }
                finally
                {
                    handle.Free();
                }

                WriteNativeStringAttr(dsetId, "CourseId", item.CourseId);
                WriteNativeStringAttr(dsetId, "PlanId", item.PlanId);
                WriteNativeStringAttr(dsetId, "ROIName", item.ROIName);
                WriteNativeStringAttr(dsetId, "Piz", item.Piz);
                WriteNativeStringAttr(dsetId, "SourceFile", item.SourceFile);
                WriteNativeDoubleAttr(dsetId, "FractionsNumber", item.FractionsNumber);
                WriteNativeDoubleAttr(dsetId, "VoxelVolume", item.VoxelVolume);
                WriteNativeStringAttr(dsetId, "VolumeUnits", item.VolumeUnits);
                WriteNativeStringAttr(dsetId, "DoseUnits", item.DoseUnits);
            }
            finally
            {
                if (dsetId >= 0) H5D.close(dsetId);
                if (spaceId >= 0) H5S.close(spaceId);
                if (dcplId >= 0) H5P.close(dcplId);
            }
        }

        private static void AppendPlanSumGroup(long fileId, PlanSumSpec spec)
        {
            ValidatePlanSum(spec, out var componentIds, out var componentPaths, out var weights, out var planSumPath);

            EnsureNativeGroups(fileId, planSumPath);

            long groupId = H5G.open(fileId, planSumPath);
            if (groupId < 0) throw new IOException($"Failed to open group '{planSumPath}'.");

            try
            {
                WriteNativeStringArrayAttr(groupId, "ComponentPlanIds", componentIds);
                WriteNativeStringArrayAttr(groupId, "ComponentPlanPaths", componentPaths);
                WriteNativeDoubleArrayAttr(groupId, "Weights", weights);
            }
            finally
            {
                H5G.close(groupId);
            }
        }

        private static void EnsureNativeGroups(long fileId, string groupPath)
        {
            var parts = groupPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string current = "";

            foreach (var part in parts)
            {
                current += "/" + part;
                if (!NativeLinkExists(fileId, current))
                {
                    long gid = H5G.create(fileId, current);
                    if (gid < 0) throw new IOException($"Failed to create group: {current}");
                    H5G.close(gid);
                }
            }
        }

        private static bool NativeLinkExists(long locId, string path) => H5L.exists(locId, path) > 0;

        private static long CreateCompressedDatasetProperties(ulong length)
        {
            if (length == 0)
                throw new ArgumentException("Dose values must contain at least one element to enable compression.", nameof(length));

            long dcplId = H5P.create(H5P.DATASET_CREATE);
            if (dcplId < 0) throw new IOException("Failed to create dataset creation property list.");

            try
            {
                var chunk = Math.Min(length, 4096UL);
                if (chunk == 0) chunk = 1;

                ulong[] chunkDims = { chunk };
                var status = H5P.set_chunk(dcplId, 1, chunkDims);
                if (status < 0) throw new IOException("Failed to configure chunked layout.");

                status = H5P.set_deflate(dcplId, 6);
                if (status < 0) throw new IOException("Failed to enable gzip compression.");

                return dcplId;
            }
            catch
            {
                H5P.close(dcplId);
                throw;
            }
        }

        private static void WriteNativeDoubleAttr(long objId, string name, double value)
        {
            long spaceId = H5S.create(H5S.class_t.SCALAR);
            if (spaceId < 0) throw new IOException("Failed to create scalar dataspace.");

            long attrId = -1;
            GCHandle handle = default;

            try
            {
                if (H5A.exists(objId, name) > 0) H5A.delete(objId, name);

                attrId = H5A.create(objId, name, H5T.NATIVE_DOUBLE, spaceId);
                if (attrId < 0) throw new IOException($"Failed to create attribute: {name}");

                var buffer = new[] { value };
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                var status = H5A.write(attrId, H5T.NATIVE_DOUBLE, handle.AddrOfPinnedObject());
                if (status < 0) throw new IOException($"Failed to write attribute: {name}");
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                if (attrId >= 0) H5A.close(attrId);
                if (spaceId >= 0) H5S.close(spaceId);
            }
        }

        private static void WriteNativeStringAttr(long objId, string name, string value)
        {
            if (value == null) value = string.Empty;

            long typeId = H5T.copy(H5T.C_S1);
            H5T.set_size(typeId, H5T.VARIABLE);
            H5T.set_cset(typeId, H5T.cset_t.UTF8);

            long spaceId = H5S.create(H5S.class_t.SCALAR);

            long attrId = -1;
            GCHandle handle = default;
            IntPtr bufferPtr = IntPtr.Zero;

            try
            {
                if (H5A.exists(objId, name) > 0) H5A.delete(objId, name);

                attrId = H5A.create(objId, name, typeId, spaceId);
                if (attrId < 0) throw new IOException($"Failed to create attribute: {name}");

                var bytes = System.Text.Encoding.UTF8.GetBytes(value);
                bufferPtr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, bufferPtr, bytes.Length);
                Marshal.WriteByte(bufferPtr, bytes.Length, 0);

                IntPtr[] arr = { bufferPtr };
                handle = GCHandle.Alloc(arr, GCHandleType.Pinned);

                var status = H5A.write(attrId, typeId, handle.AddrOfPinnedObject());
                if (status < 0) throw new IOException($"Failed to write attribute: {name}");
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                if (bufferPtr != IntPtr.Zero) Marshal.FreeHGlobal(bufferPtr);
                if (attrId >= 0) H5A.close(attrId);
                if (spaceId >= 0) H5S.close(spaceId);
                if (typeId >= 0) H5T.close(typeId);
            }
        }

        private static void WriteNativeStringArrayAttr(long objId, string name, IEnumerable<string> values)
        {
            var list = values?.Where(v => !string.IsNullOrWhiteSpace(v))
                              .Select(v => v.Trim())
                              .ToList() ?? new List<string>();

            if (list.Count == 0)
            {
                if (H5A.exists(objId, name) > 0) H5A.delete(objId, name);
                return;
            }

            long typeId = H5T.copy(H5T.C_S1);
            H5T.set_size(typeId, H5T.VARIABLE);
            H5T.set_cset(typeId, H5T.cset_t.UTF8);

            ulong[] dims = { (ulong)list.Count };
            long spaceId = H5S.create_simple(1, dims, null);

            long attrId = -1;
            GCHandle handle = default;
            var ptrs = new IntPtr[list.Count];

            try
            {
                if (H5A.exists(objId, name) > 0) H5A.delete(objId, name);

                attrId = H5A.create(objId, name, typeId, spaceId);
                if (attrId < 0) throw new IOException($"Failed to create attribute: {name}");

                for (int i = 0; i < list.Count; i++)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(list[i]);
                    ptrs[i] = Marshal.AllocHGlobal(bytes.Length + 1);
                    Marshal.Copy(bytes, 0, ptrs[i], bytes.Length);
                    Marshal.WriteByte(ptrs[i], bytes.Length, 0);
                }

                handle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);
                var status = H5A.write(attrId, typeId, handle.AddrOfPinnedObject());
                if (status < 0) throw new IOException($"Failed to write attribute: {name}");
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                foreach (var ptr in ptrs)
                {
                    if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
                }
                if (attrId >= 0) H5A.close(attrId);
                if (spaceId >= 0) H5S.close(spaceId);
                if (typeId >= 0) H5T.close(typeId);
            }
        }

        private static void WriteNativeDoubleArrayAttr(long objId, string name, IEnumerable<double> values)
        {
            var list = values?.ToList() ?? new List<double>();

            if (list.Count == 0)
            {
                if (H5A.exists(objId, name) > 0) H5A.delete(objId, name);
                return;
            }

            long spaceId = H5S.create_simple(1, new[] { (ulong)list.Count }, null);
            if (spaceId < 0) throw new IOException("Failed to create dataspace for attribute: " + name);

            long attrId = -1;
            GCHandle handle = default;

            try
            {
                if (H5A.exists(objId, name) > 0) H5A.delete(objId, name);

                attrId = H5A.create(objId, name, H5T.NATIVE_DOUBLE, spaceId);
                if (attrId < 0) throw new IOException("Failed to create attribute: " + name);

                var buffer = list.ToArray();
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var status = H5A.write(attrId, H5T.NATIVE_DOUBLE, handle.AddrOfPinnedObject());
                if (status < 0) throw new IOException("Failed to write attribute: " + name);
            }
            finally
            {
                if (handle.IsAllocated) handle.Free();
                if (attrId >= 0) H5A.close(attrId);
                if (spaceId >= 0) H5S.close(spaceId);
            }
        }

        private static List<string> ReadNativeStringArrayAttr(long objId, string name)
        {
            var result = new List<string>();
            if (H5A.exists(objId, name) <= 0) return result;

            long attrId = -1;
            long spaceId = -1;
            long typeId = -1;
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
                    var str = PtrToStringUtf8(buffer[i]);
                    if (!string.IsNullOrWhiteSpace(str))
                        result.Add(str.Trim());
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

        private static List<string> MergeStringLists(IEnumerable<string> existing, IEnumerable<string> additions)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            void AddRange(IEnumerable<string> source)
            {
                if (source == null) return;
                foreach (var value in source)
                {
                    var trimmed = value?.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    if (seen.Add(trimmed))
                        result.Add(trimmed);
                }
            }

            AddRange(existing);
            if (additions != null)
                AddRange(additions.OrderBy(v => v, StringComparer.Ordinal));

            return result;
        }

        private static void Validate(DoseDatasetSpec item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!string.IsNullOrWhiteSpace(item.DosePath))
            {
                if (!item.DosePath.StartsWith("/") || !item.DosePath.EndsWith("/dose"))
                    throw new ArgumentException("DosePath must be an absolute HDF5 path ending with '/dose'.");
            }

            if (item.Values == null)
                throw new ArgumentNullException(nameof(item.Values), "Values cannot be null.");

            if (string.IsNullOrWhiteSpace(item.Piz))
                throw new ArgumentException("Piz cannot be null or whitespace.", nameof(item));
            if (string.IsNullOrWhiteSpace(item.CourseId))
                throw new ArgumentException("CourseId cannot be null or whitespace.", nameof(item));
            if (string.IsNullOrWhiteSpace(item.PlanId))
                throw new ArgumentException("PlanId cannot be null or whitespace.", nameof(item));
            if (string.IsNullOrWhiteSpace(item.ROIName))
                throw new ArgumentException("ROIName cannot be null or whitespace.", nameof(item));
        }

        private static string GetDosePath(DoseDatasetSpec item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (!string.IsNullOrWhiteSpace(item.DosePath))
                return item.DosePath;

            var patientSegment = item.Piz?.Trim('/') ?? throw new ArgumentException("Piz is required to build the dose path.", nameof(item));
            var courseSegment = item.CourseId?.Trim('/') ?? throw new ArgumentException("CourseId is required to build the dose path.", nameof(item));
            var planSegment = item.PlanId?.Trim('/') ?? throw new ArgumentException("PlanId is required to build the dose path.", nameof(item));
            var roiSegment = item.ROIName?.Trim('/') ?? throw new ArgumentException("ROIName is required to build the dose path.", nameof(item));

            return $"/patients/{patientSegment}/courses/{courseSegment}/plans/{planSegment}/rois/{roiSegment}/dose";
        }

        private static void ValidatePlanSum(PlanSumSpec spec, out List<string> componentIds, out List<string> componentPaths, out List<double> weights, out string planSumPath)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (string.IsNullOrWhiteSpace(spec.PlanSumId))
                throw new ArgumentException("PlanSumId cannot be null or whitespace.", nameof(spec));
            if (spec.PlanSumId.Contains("..") || spec.PlanSumId.Contains("/"))
                throw new ArgumentException("PlanSumId must not contain path separators.", nameof(spec));

            if (string.IsNullOrWhiteSpace(spec.PatientId))
                throw new ArgumentException("PatientId cannot be null or whitespace.", nameof(spec));
            if (string.IsNullOrWhiteSpace(spec.CourseId))
                throw new ArgumentException("CourseId cannot be null or whitespace.", nameof(spec));

            var tempComponentIds = new List<string>();
            var tempComponentPaths = new List<string>();
            void AddComponent(string patient, string course, string planId)
            {
                if (string.IsNullOrWhiteSpace(planId))
                    throw new ArgumentException("Component plan must have a PlanId.", nameof(spec));

                var patientSegment = string.IsNullOrWhiteSpace(patient) ? spec.PatientId : patient;
                var courseSegment = string.IsNullOrWhiteSpace(course) ? spec.CourseId : course;

                tempComponentIds.Add(planId.Trim());
                tempComponentPaths.Add(BuildPlanPath(patientSegment, courseSegment, planId));
            }

            if (spec.ComponentPlans != null)
            {
                foreach (var plan in spec.ComponentPlans)
                {
                    if (plan == null) continue;
                    AddComponent(plan.Piz, plan.CourseId, plan.PlanId);
                }
            }

            if (spec.ComponentPlanIds != null)
            {
                foreach (var id in spec.ComponentPlanIds)
                {
                    var trimmed = id?.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        AddComponent(spec.PatientId, spec.CourseId, trimmed);
                }
            }

            if (tempComponentIds.Count == 0)
                throw new ArgumentException("At least one ComponentPlanId is required.", nameof(spec));

            if (spec.Weights != null)
            {
                weights = new List<double>();
                foreach (var weight in spec.Weights)
                {
                    weights.Add(weight);
                }

                if (weights.Count != tempComponentIds.Count)
                    throw new ArgumentException("Weights count must match ComponentPlanIds count.", nameof(spec));
            }
            else
            {
                weights = new List<double>(tempComponentIds.Count);
                for (int i = 0; i < tempComponentIds.Count; i++)
                    weights.Add(1.0);
            }

            componentIds = tempComponentIds;
            componentPaths = tempComponentPaths;
            planSumPath = BuildPlanSumPath(spec.PatientId, spec.CourseId, spec.PlanSumId);
        }

        private static string BuildPlanPath(string patientId, string courseId, string planId)
        {
            var patientSegment = (patientId ?? string.Empty).Trim('/');
            var courseSegment = (courseId ?? string.Empty).Trim('/');
            var planSegment = (planId ?? string.Empty).Trim('/');

            if (string.IsNullOrEmpty(patientSegment) || string.IsNullOrEmpty(courseSegment) || string.IsNullOrEmpty(planSegment))
                throw new ArgumentException("PatientId, CourseId, and PlanId must be provided.");

            return $"/patients/{patientSegment}/courses/{courseSegment}/plans/{planSegment}";
        }

        private static string BuildPlanSumPath(string patientId, string courseId, string planSumId)
        {
            var patientSegment = (patientId ?? string.Empty).Trim('/');
            var courseSegment = (courseId ?? string.Empty).Trim('/');
            var planSumSegment = (planSumId ?? string.Empty).Trim('/');

            if (string.IsNullOrEmpty(patientSegment) || string.IsNullOrEmpty(courseSegment) || string.IsNullOrEmpty(planSumSegment))
                throw new ArgumentException("PatientId, CourseId, and PlanSumId must be provided.");

            return $"/patients/{patientSegment}/courses/{courseSegment}/plan_sums/{planSumSegment}";
        }

        private sealed class PatientMetadataAggregator
        {
            private readonly Dictionary<string, PatientMetadata> _patients = new Dictionary<string, PatientMetadata>(StringComparer.Ordinal);

            public void AddDose(DoseDatasetSpec spec)
            {
                if (spec == null) return;
                var patientId = spec.Piz?.Trim();
                if (string.IsNullOrEmpty(patientId)) return;

                var metadata = GetOrCreate(patientId);
                metadata.AddCourse(spec.CourseId);
                metadata.AddPlan(spec.PlanId);
                metadata.AddRoi(spec.ROIName);
            }

            public void AddPlanSum(PlanSumSpec spec)
            {
                if (spec == null) return;
                var patientId = spec.PatientId?.Trim();
                if (string.IsNullOrEmpty(patientId)) return;

                var metadata = GetOrCreate(patientId);
                metadata.AddCourse(spec.CourseId);
                metadata.AddPlanSum(spec.PlanSumId);

                if (spec.ComponentPlanIds != null)
                {
                    foreach (var planId in spec.ComponentPlanIds)
                        metadata.AddPlan(planId);
                }

                if (spec.ComponentPlans != null)
                {
                    foreach (var plan in spec.ComponentPlans)
                    {
                        if (plan == null) continue;
                        metadata.AddPlan(plan.PlanId);
                    }
                }
            }

            public IEnumerable<KeyValuePair<string, PatientMetadata>> Items => _patients;

            private PatientMetadata GetOrCreate(string patientId)
            {
                if (!_patients.TryGetValue(patientId, out var metadata))
                {
                    metadata = new PatientMetadata();
                    _patients[patientId] = metadata;
                }

                return metadata;
            }
        }

        private sealed class PatientMetadata
        {
            public HashSet<string> CourseNames { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> PlanNames { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> PlanSumIds { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> ROINames { get; } = new HashSet<string>(StringComparer.Ordinal);

            public void AddCourse(string value) => AddValue(CourseNames, value);
            public void AddPlan(string value) => AddValue(PlanNames, value);
            public void AddPlanSum(string value) => AddValue(PlanSumIds, value);
            public void AddRoi(string value) => AddValue(ROINames, value);

            private static void AddValue(HashSet<string> target, string value)
            {
                var trimmed = value?.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    target.Add(trimmed);
            }
        }
    }
}