﻿// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Radiobiology.LQ
{
    public interface IEquieffectiveDoseConverter
    {
        double ComputeEqd0(double totalDose);
        double ComputeEqd2(double totalDose);
    }
}