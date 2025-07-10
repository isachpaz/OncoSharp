// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using NLoptNet;
using OncoSharp.Optimization.Abstractions.Models;

namespace OncoSharp.Optimization.Algorithms.NLopt
{
    
    namespace OncoSharp.Optimization.Algorithms.NLopt
    {
        public static class NloptResultMapper
        {
            public static OptimizerExitStatus MapToExitStatus(NloptResult result)
            {
                switch (result)
                {
                    case NloptResult.SUCCESS:
                        return OptimizerExitStatus.Success;
                    case NloptResult.STOPVAL_REACHED:
                    case NloptResult.FTOL_REACHED:
                    case NloptResult.XTOL_REACHED:
                        return OptimizerExitStatus.ConvergenceReached;
                    case NloptResult.MAXEVAL_REACHED:
                        return OptimizerExitStatus.MaxEvaluationsReached;
                    case NloptResult.MAXTIME_REACHED:
                        return OptimizerExitStatus.MaxTimeReached;
                    case NloptResult.ROUNDOFF_LIMITED:
                        return OptimizerExitStatus.RoundoffLimited;
                    case NloptResult.FORCED_STOP:
                        return OptimizerExitStatus.ForcedStop;
                    case NloptResult.INVALID_ARGS:
                        return OptimizerExitStatus.InvalidArguments;
                    case NloptResult.FAILURE:
                    case NloptResult.OUT_OF_MEMORY:
                        return OptimizerExitStatus.Failure;
                    default:
                        return OptimizerExitStatus.Unknown;
                }
            }
        }
    }
}