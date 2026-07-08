// <copyright project="NZCore.Math" file="MathProcessResult.cs">
// Copyright © 2026 Thomas Enzenebner. All rights reserved.
// </copyright>

namespace NZCore
{
    /// <summary>
    /// Result flags of <c>MathOperator.ProcessCapped</c>.
    /// <c>Changed</c>: the stored value changed.
    /// <c>AtMin</c>: the final value is <= 0.
    /// <c>AtMax</c>: a cap pointer was supplied and the final value is >= the cap.
    /// </summary>
    public struct MathProcessResult
    {
        public bool Changed;
        public bool AtMin;
        public bool AtMax;
    }
}
