// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using OncoSharp.Optimization.Abstractions.Interfaces;

namespace OncoSharp.Statistics.Abstractions.Interfaces
{
    internal interface IMleInternals<TData, TParameters>
    {
        double[] GetLowerBounds();
        double[] GetUpperBounds();
        double LogLikelihood(TParameters parameters, IList<bool> observations, IList<TData> inputData);
        TParameters ConvertVectorToParameters(double[] parameters);
        IOptimizer CreateSolver(int parameterCount);
    }
}