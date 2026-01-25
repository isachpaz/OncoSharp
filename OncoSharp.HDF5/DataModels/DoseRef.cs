// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;

namespace OncoSharp.HDF5.DataModels
{
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
}