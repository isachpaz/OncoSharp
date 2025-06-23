// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Threading;

namespace OncoSharp.Core.Quantities
{
    public static class QuantityValidationConfig
    {
        private static readonly AsyncLocal<bool> _enforcePositive = new AsyncLocal<bool> { Value = true };

        public static bool EnforcePositive
        {
            get => _enforcePositive.Value;
            set => _enforcePositive.Value = value;
        }
    }
}