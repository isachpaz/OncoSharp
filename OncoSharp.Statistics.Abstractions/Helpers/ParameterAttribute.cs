// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;

namespace OncoSharp.Statistics.Abstractions.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        public int Order { get; }

        public ParameterAttribute(int order)
        {
            Order = order;
        }
    }
}