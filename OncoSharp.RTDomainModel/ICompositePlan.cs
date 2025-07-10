// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using System.Collections.Generic;

namespace OncoSharp.RTDomainModel
{
    public interface ICompositePlan : IPlanItem
    {
        IEnumerable<IPlanItem> GetChildPlans(); 
        void AddPlan(IPlanItem plan);
        bool CanAddPlan(IPlanItem plan);
    }
}