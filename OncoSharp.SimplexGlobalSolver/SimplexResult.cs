// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;
using NLoptNet;

namespace OncoSharp.SimplexGlobalSolver
{
    public class SimplexResult
    {
        public List<double> Points { get; }
        public double ObjectiveValue { get; }
        public NloptResult ExitReason { get; }

        public SimplexResult(
            IEnumerable<double> points,
            double objectiveValue,
            NloptResult exitReason)
        {

            Points = new List<double>(points);
            ObjectiveValue = objectiveValue;
            ExitReason = exitReason;
        }

        public override string ToString()
        {
            return
                $"{nameof(Points)}: {Points}, {nameof(ObjectiveValue)}: {ObjectiveValue}, {nameof(ExitReason)}: {ExitReason}";
        }
    }
}