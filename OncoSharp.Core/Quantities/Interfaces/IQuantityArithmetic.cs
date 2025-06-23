// // OncoSharp
// // Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// // Licensed for non-commercial academic and research use only.
// // Commercial use requires a separate license.
// // See https://github.com/isachpaz/OncoSharp for more information.

namespace OncoSharp.Core.Quantities.Interfaces
{
    public interface IQuantityArithmetic<TValue>
    {
        TValue Add(TValue valueT);
        TValue Subtract(TValue valueT);
        TValue Multiply(double dValue);
    }
}