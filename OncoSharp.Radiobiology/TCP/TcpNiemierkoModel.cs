// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using OncoSharp.Core.Quantities.CloudPoint;
using OncoSharp.Core.Quantities.DimensionlessValues;
using OncoSharp.Core.Quantities.Dose;
using OncoSharp.Core.Quantities.Probability;
using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Radiobiology.GEUD;

namespace OncoSharp.Radiobiology.TCP
{
    public class TcpNiemierkoModel
    {
        public Geud2GyModel GeudModel { get; }
        public EQD2Value D50 { get; }
        public GammaValue Gamma50 { get; }

        public TcpNiemierkoModel(Geud2GyModel geudModel, EQD2Value d50, GammaValue gamma50)
        {
            GeudModel = geudModel;
            D50 = d50;
            Gamma50 = gamma50;
        }
        
        
        public virtual ProbabilityValue ComputeTcp(DoseCloudPoints<EQD2Value> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));

            var totalVolume = points.TotalVolume;
            
            var geud2Gy = GeudModel.Calculate(points);
            var d50DividedByGEUD2Gy = D50 / geud2Gy;
            var tcp = ProbabilityValue.New(1.0 / (1.0 + Math.Pow(d50DividedByGEUD2Gy, 4 * Gamma50)));
            
            return tcp;
        }
    }
}