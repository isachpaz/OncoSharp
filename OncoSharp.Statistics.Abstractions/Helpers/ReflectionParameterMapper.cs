// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Linq;
using System.Reflection;
using OncoSharp.Statistics.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Abstractions.Helpers
{
    public class ReflectionParameterMapper<T> : IParameterMapper<T> where T : new()
    {
        private readonly PropertyInfo[] _orderedProperties;

        public ReflectionParameterMapper()
        {
            _orderedProperties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(double))
                .Select(p => new
                {
                    Property = p,
                    p.GetCustomAttribute<ParameterAttribute>()?.Order,
                })
                .OrderBy(p => p.Order ?? int.MaxValue) // Put unordered at the end
                .ThenBy(p => p.Property.Name) // Fallback to name ordering
                .Select(p => p.Property)
                .ToArray();
        }

        public string[] ParameterNames => _orderedProperties.Select(p => p.Name).ToArray();

        public double[] ToArray(T parameters)
        {
            return _orderedProperties
                .Select(p => (double)p.GetValue(parameters))
                .ToArray();
        }

        public T FromArray(double[] array)
        {
            if (array.Length != _orderedProperties.Length)
                throw new ArgumentException(
                    $"Expected vector of length {_orderedProperties.Length}, got {array.Length}");

            var instance = new T();

            for (int i = 0; i < _orderedProperties.Length; i++)
            {
                _orderedProperties[i].SetValue(instance, array[i]);
            }

            return instance;
        }
    }
}