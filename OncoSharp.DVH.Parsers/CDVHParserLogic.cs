// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Factories;

namespace OncoSharp.DVH.Parsers
{
    public static class CDVHParser
    {
        public static CDVHFile Parse(string path)
        {
            var lines = File.ReadAllLines(path);
            var result = new CDVHFile();
            CDVHStructure currentStructure = null;

            bool parsingDVH = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                if (parsingDVH)
                {
                    if (Regex.IsMatch(trimmed, @"^\d"))
                    {
                        var parts = Regex.Split(trimmed, @"\s+");
                        if (parts.Length >= 3 &&
                            double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture,
                                out double dose) &&
                            double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture,
                                out double relDose) &&
                            double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double vol))
                        {
                            currentStructure?.Entries.Add(new CDVHEntry
                            {
                                DoseGy = dose,
                                RelativeDosePercent = relDose,
                                VolumePercent = vol
                            });
                        }
                    }

                    continue;
                }

                // Header parsing
                if (line.StartsWith("Patient Name"))
                    result.PatientName = ExtractValue(line);
                else if (line.StartsWith("Patient ID"))
                    result.PatientID = ExtractValue(line);
                else if (line.StartsWith("Comment"))
                    result.Comment = ExtractValue(line);
                else if (line.StartsWith("Date"))
                    result.Date = ExtractValue(line);
                else if (line.StartsWith("Exported by"))
                    result.ExportedBy = ExtractValue(line);
                else if (line.StartsWith("Type"))
                    result.Type = ExtractValue(line);
                else if (line.StartsWith("Description"))
                    result.Description = ExtractValue(line);
                else if (line.StartsWith("Plan:"))
                    result.Plan = ExtractValue(line);
                else if (line.StartsWith("Course:"))
                    result.Course = ExtractValue(line);
                else if (line.StartsWith("Plan Status:"))
                    result.PlanStatus = ExtractValue(line);
                else if (line.StartsWith("Total dose"))
                    result.TotalDoseGy = ParseDouble(ExtractValue(line));

                // Structure start
                else if (line.StartsWith("Structure:"))
                {
                    currentStructure = new CDVHStructure { Name = ExtractValue(line) };
                    result.Structures.Add(currentStructure);
                }

                // Structure-specific info
                else if (currentStructure != null && line.Contains(":"))
                {
                    var key = line.Split(':')[0].Trim();
                    var value = ExtractValue(line);

                    if (key == "Volume [cm³]") currentStructure.VolumeCm3 = ParseDouble(value);
                    currentStructure.Stats[key] = value;
                }

                // Start of DVH table
                else if (line.Contains("Dose [Gy]") && line.Contains("Volume"))
                {
                    parsingDVH = true;
                }
            }

            result.DVHs.Add(ConvertToDVH(currentStructure));
            return result;
        }

        private static IDVHBase ConvertToDVH(CDVHStructure currentStructure)
        {
            VolumeValue totalVolume = currentStructure.VolumeCm3.cm3();
            List<DVHPoint> dvhPoints = new List<DVHPoint>();

            foreach (CDVHEntry item in currentStructure.Entries)
            {
                dvhPoints.Add(new DVHPoint(item.DoseGy, VolumeValue.InPercent(item.VolumePercent)));
            }

            double maxDose = (double)Double.Parse(currentStructure.Stats["Max Dose [Gy]"]);
            double minDose = (double)Double.Parse(currentStructure.Stats["Min Dose [Gy]"]);
            return CDVHFactory.FromCumulativeDVHPoints(dvhPoints, maxDose, minDose, totalVolume, DoseUnit.Gy);
        }


        private static string ExtractValue(string line)
        {
            var idx = line.IndexOf(':');
            return idx >= 0 ? line.Substring(idx + 1).Trim() : string.Empty;
        }

        private static double ParseDouble(string input)
        {
            return double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : double.NaN;
        }
    }
}