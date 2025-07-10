// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Runtime.CompilerServices;

namespace OncoSharp.Statistics.Abstractions.Helpers
{
    public static class MathUtils
    {
        
        internal static void ThrowMinMaxException<T>(T min, T max)
        {
            throw new ArgumentException($"{min} is larger than {max}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                ThrowMinMaxException(min, max);
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

    }
}