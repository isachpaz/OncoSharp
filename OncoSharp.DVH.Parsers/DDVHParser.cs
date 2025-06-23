// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Volume;
using OncoSharp.DVH.Factories;

namespace OncoSharp.DVH.Parsers
{
    namespace DVHLib.Parsers
    {
        public static class DDVHParser
        {
            public static DDVHFile Parse(string path)
            {
                var lines = File.ReadAllLines(path);
                var result = new DDVHFile();

                DDVHStructure currentStructure = null;
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
                                double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture,
                                    out double volPerGy))
                            {
                                currentStructure?.Entries.Add(new DDVHEntry
                                {
                                    DoseGy = dose,
                                    RelativeDosePercent = relDose,
                                    VolumePerDoseCm3PerGy = volPerGy
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
                        currentStructure = new DDVHStructure { Name = ExtractValue(line) };
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
                    else if (line.Contains("Dose [Gy]") && line.Contains("dVolume") && currentStructure != null)
                    {
                        parsingDVH = true;
                    }
                }

                // Convert and attach DVHs
                foreach (var structure in result.Structures)
                {
                    result.DVHs.Add(ConvertToDVH(structure));
                }

                return result;
            }

            public static double SmartRound(double value, params double[] inputs)
            {
                int maxDecimals = 0;
                foreach (double num in inputs)
                {
                    string str = num.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    int decimalIndex = str.IndexOf('.');
                    if (decimalIndex >= 0)
                    {
                        int decimals = str.Length - decimalIndex - 1;
                        if (decimals > maxDecimals)
                            maxDecimals = decimals;
                    }
                }

                return Math.Round(value, maxDecimals);
            }


            private static IDVHBase ConvertToDVH(DDVHStructure currentStructure)
            {
                VolumeValue totalVolume = currentStructure.VolumeCm3.cm3();
                List<DVHPoint> dvhPoints = new List<DVHPoint>();
                double binWidth = SmartRound(currentStructure.Entries[1].DoseGy - currentStructure.Entries[0].DoseGy,
                    currentStructure.Entries[1].DoseGy,
                    currentStructure.Entries[0].DoseGy);

                foreach (DDVHEntry item in currentStructure.Entries)
                {
                    VolumeValue
                        vol = VolumeValue.InCM3(item.VolumePerDoseCm3PerGy *
                                                binWidth); // Interpreted per Gy, accumulate or rebin later if needed
                    dvhPoints.Add(new DVHPoint(item.DoseGy, vol));
                }

                double maxDose = currentStructure.Stats.TryGetValue("Max Dose [Gy]", out var maxStr)
                    ? ParseDouble(maxStr)
                    : dvhPoints.Max(p => p.Dose);
                double minDose = currentStructure.Stats.TryGetValue("Min Dose [Gy]", out var minStr)
                    ? ParseDouble(minStr)
                    : dvhPoints.Min(p => p.Dose);
                var calcVolume = dvhPoints.Sum(p => p.Volume.Value);

                return DDVHFactory.FromDifferentialDVHPoints(dvhPoints, maxDose, minDose, totalVolume, DoseUnit.Gy);
            }

            private static string ExtractValue(string line)
            {
                var idx = line.IndexOf(':');
                return idx >= 0 ? line.Substring(idx + 1).Trim() : string.Empty;
            }

            private static double ParseDouble(string input)
            {
                return double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                    ? d
                    : double.NaN;
            }
        }
    }
}