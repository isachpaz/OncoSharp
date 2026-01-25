using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Extensions;
using OncoSharp.Core.Quantities.Fractions;
using OncoSharp.RTDomainModel;

namespace OncoSharp.Statistics.Models.Tests
{
    public class MockPlanItem : IPlanItem
    {
        private readonly double _eqd2;

        public MockPlanItem(double eqd2)
        {
            _eqd2 = eqd2;
            IsValid = true;
        }

        public string PatientId => "MockPatient";
        public string PlanId => "MockPlan";
        public DoseValue PrescriptionDose => DoseValue.InGy(70);
        public FractionsValue Fractions => new FractionsValue(35);
        public bool IsValid { get; }

        public DoseCloudPoints<EQD2Value> CalculateEqd2DoseDistribution(string structureId, DoseValue abRatio)
        {
            var points = new List<EQD2Value>();
            for (int i = 0; i < 100; i++)
            {
                points.Add(EQD2Value.InGy(_eqd2));
            }

            var items = points.Select(x => new DoseCloudPoint<EQD2Value>(x, 1.cm3())).ToList();
            return new DoseCloudPoints<EQD2Value>(items);
        }

        public DoseCloudPoints<EQD0Value> CalculateEqd0DoseDistribution(string structureId, DoseValue abRatio)
        {
            throw new NotImplementedException();
        }

    }
}
