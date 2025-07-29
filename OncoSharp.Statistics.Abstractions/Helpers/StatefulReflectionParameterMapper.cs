//// OncoSharp
//// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
//// Licensed for non-commercial academic and research use only.
//// Commercial use requires a separate license.
//// See https://github.com/isachpaz/OncoSharp for more information.

//using OncoSharp.Statistics.Abstractions.Interfaces;
//using System;

//namespace OncoSharp.Statistics.Abstractions.Helpers
//{
//    public class StatefulReflectionParameterMapper<T> : IParameterMapper<T> where T : new()
//    {
//        private readonly ReflectionParameterMapper<T> _helper = new ReflectionParameterMapper<T>();

//        private T _parameters = new T();
//        private double[] _array;

//        public StatefulReflectionParameterMapper() { }

//        // Set internal array and create parameters from it
//        public void SetArray(double[] array)
//        {
//            _array = array;
//            _parameters = _helper.FromArray(array);
//        }

//        // Set internal parameters
//        public void SetParameters(T parameters)
//        {
//            _parameters = parameters;
//            _array = _helper.ToArray(parameters);
//        }

//        public T FromArray()
//        {
//            if (_array == null)
//                throw new InvalidOperationException("No array set");
//            return _helper.FromArray(_array);
//        }

//        public double[] ToArray()
//        {
//            if (_parameters == null)
//                throw new InvalidOperationException("No parameters set");
//            return _helper.ToArray(_parameters);
//        }

//        public int GetParametersCount() => _helper.GetParametersCount();

//        public string[] ParameterNames => _helper.ParameterNames;
//    }
//}