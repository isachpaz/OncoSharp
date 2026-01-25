// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;

namespace OncoSharp.HDF5.DataModels
{
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
}