// <copyright project="NZCore.Math" file="IGenericMathAction.cs">
// Copyright © 2026 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;

namespace NZCore
{
    /// <summary>
    /// Work that runs on a value type only known at runtime. Implement it as a struct and pass it to
    /// <see cref="GenericMath.Dispatch{TAction}"/>, which calls <see cref="Invoke{T,TProcessor}"/> with the C# type and processor
    /// of a <see cref="GenericDataType"/>. Burst resolves and inlines the call per type.
    /// </summary>
    public interface IGenericMathAction
    {
        void Invoke<T, TProcessor>()
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>;
    }
}