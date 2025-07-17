// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities;
using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Helpers.Maths;
using OncoSharp.Core.Quantities.Probability;
using OncoSharp.Radiobiology.GEUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OncoSharp.Radiobiology.TCP
{
    public class TcpProbitModel
    {
        public Geud2GyModel GeudModel { get; }
        public double D50 { get; }
        public double Gamma50 { get; }


        public TcpProbitModel(double d50, double gamma50)
        {
            D50 = d50;
            Gamma50 = gamma50;
        }

       public virtual ProbabilityValue ComputeTcp(List<DoseCloudPoint<EQD2Value>> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            
            var geud2Gy = GeudModel.Calculate(points);
            var response = Gamma50 * Math.Sqrt(Math.PI) * (1.0 - geud2Gy.Value / D50);
            response = 0.5 * (1.0 - MathUtils.Erf(response));

            if (Double.IsNaN(response))
            {
                Debug.WriteLine("ComputeVoxelResponse was NaN. Please, check further!");
                response = 0.0;
            }

            return ProbabilityValue.New(response);
        }
    }
}