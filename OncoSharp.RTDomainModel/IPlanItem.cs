// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Fractions;
using OncoSharp.Core.Quantities.Volume;

namespace OncoSharp.RTDomainModel
{
    public interface IPlanItem
    {
        string PatientId { get; }
        string PlanId { get; }

        DoseValue PrescriptionDose { get; }
        FractionsValue Fractions { get; }

        bool IsValid { get; }

        DoseCloudPoints<EQD2Value> CalculateEqd2DoseDistribution(string structureId, DoseValue abRatio);
        DoseCloudPoints<EQD0Value> CalculateEqd0DoseDistribution(string structureId, DoseValue abRatio);

        VolumeValue GetStructureVolume(string structureId);
    }
}