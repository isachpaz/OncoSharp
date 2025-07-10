// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Optimization.Abstractions.Models
{

    public enum OptimizerExitStatus
    {
        Success,
        Failure,
        InvalidArguments,
        ConvergenceReached,
        MaxEvaluationsReached,
        MaxTimeReached,
        RoundoffLimited,
        ForcedStop,
        Unknown
    }

    public class OptimizationResult
    {
        public double[] OptimizedParameters { get; }
        public double ObjectiveValue { get; }
        public OptimizerExitStatus Status { get; }

        public OptimizationResult(
            double[] optimizedParameters,
            double objectiveValue,
            OptimizerExitStatus status)
        {
            OptimizedParameters = optimizedParameters;
            ObjectiveValue = objectiveValue;
            Status = status;
        }

        public override string ToString()
        {
            return
                $"{nameof(OptimizedParameters)}: {OptimizedParameters}, {nameof(ObjectiveValue)}: {ObjectiveValue}, {nameof(Status)}: {Status}";
        }
    }
}