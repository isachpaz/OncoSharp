// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;

namespace OncoSharp.DVH.Parsers
{
    public class CDVHEntry
    {
        public double DoseGy { get; set; }
        public double RelativeDosePercent { get; set; }
        public double VolumePercent { get; set; }
    }

    public class CDVHStructure
    {
        public string Name { get; set; }
        public double VolumeCm3 { get; set; }
        public Dictionary<string, string> Stats { get; set; } = new Dictionary<string, string>();
        public List<CDVHEntry> Entries { get; set; } = new List<CDVHEntry>();
    }

    public class CDVHFile
    {
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string ExportedBy { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public string Plan { get; set; }
        public string Course { get; set; }
        public string PlanStatus { get; set; }
        public double TotalDoseGy { get; set; }

        public List<CDVHStructure> Structures { get; set; } = new List<CDVHStructure>();
        public List<IDVHBase> DVHs { get; set; } = new List<IDVHBase>();
    }

    public class DDVHFile
    {
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string ExportedBy { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public string Plan { get; set; }
        public string Course { get; set; }
        public string PlanStatus { get; set; }
        public double TotalDoseGy { get; set; }

        public List<DDVHStructure> Structures { get; set; } = new List<DDVHStructure>();
        public List<IDVHBase> DVHs { get; set; } = new List<IDVHBase>();
    }

    public class DDVHEntry
    {
        public double DoseGy { get; set; }
        public double RelativeDosePercent { get; set; }
        public double VolumePerDoseCm3PerGy { get; set; }
    }

    public class DDVHStructure
    {
        public string Name { get; set; }
        public double VolumeCm3 { get; set; }
        public Dictionary<string, string> Stats { get; set; } = new Dictionary<string, string>();
        public List<DDVHEntry> Entries { get; set; } = new List<DDVHEntry>();
    }
}