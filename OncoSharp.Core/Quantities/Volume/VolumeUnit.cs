// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OncoSharp.Core.Quantities.Volume
{
    public enum VolumeUnit
    {
        [Display(Name = "Unknown")] UNKNOWN = 0,
        [Display(Name = "mm³")] MM3, // Cubic millimeters
        [Display(Name = "cm³")] CM3, // Cubic centimeters (same as cc)
        [Display(Name = "%")] PERCENT // Relative volume (e.g., 50% of an organ)
    }

    public static class VolumeUnitDisplay
    {
        public static string Name(VolumeUnit unit)
        {
            return unit
                .GetType()
                .GetMember(unit.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? unit.ToString();
        }
    }
}