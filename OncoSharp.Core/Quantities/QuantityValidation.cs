// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System;

namespace OncoSharp.Core.Quantities
{
    public static class QuantityValidation
    {
        public static double EnsurePositiveOrThrowException(double value, string paramName)
        {
            if (QuantityValidationConfig.EnforcePositive && value < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value must be non-negative.");
            return value;
        }
    }
}