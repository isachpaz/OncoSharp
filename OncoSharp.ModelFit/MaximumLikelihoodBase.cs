using System;

namespace OncoSharp.ModelFit
{
    public abstract class MaximumLikelihoodBase
    {
        private readonly bool _bIsReportingEnabled;
        protected double _bestMaximumLikelihood = double.MinValue;
        protected double[] _bestX;


    }
}
