﻿// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.RTDomainModel
{
    public interface IStructureModelStrategy
    {
        string GetStructure(string piz, string planId);
    }
}