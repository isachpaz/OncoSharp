// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OncoSharp.Core.Quantities.Dose
{
    public enum DoseUnit
    {
        [Display(Name = "Unknown")] UNKNOWN = 0,
        [Display(Name = "Gy")] Gy = 1,
        [Display(Name = "cGy")] cGy = 2,
        [Display(Name = "%")] PERCENT = 3,
    }

    public static class DoseUnitDisplay
    {
        public static string Name(DoseUnit unit)
        {
            return unit
                .GetType()
                .GetMember(unit.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? unit.ToString();
        }
    }
}