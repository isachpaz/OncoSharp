// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Linq;

namespace OncoSharp.Core.Quantities.Helpers.Maths
{
    public static class MathUtils
    {
        public static int CountDecimalDigits(double n)
        {
            return n.ToString(System.Globalization.CultureInfo.InvariantCulture)
                //.TrimEnd('0') uncomment if you don't want to count trailing zeroes
                .SkipWhile(c => c != '.')
                .Skip(1)
                .Count();
        }
    }
}