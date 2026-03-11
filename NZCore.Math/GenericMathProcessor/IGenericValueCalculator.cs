// <copyright project="NZCore.Math" file="IGenericValueCalculator.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

namespace NZCore
{
    public interface IGenericValueCalculator<T>
        where T : unmanaged
    {
        public T Add(T a, T b) => default;
        T Subtract(T a, T b) => default;
        T Multiply(T a, T b) => default;
        T Divide(T a, T b) => default;
        T PowerAtoB(T a, T b) => default;
        T PowerBtoA(T a, T b) => default;
        T Min(T a, T b) => default;
        T Max(T a, T b) => default;

        // Math functions

        T Abs(T a) => default;
        T Ceil(T a) => default;
        T Floor(T a) => default;
        T Round(T a) => default;
        T Log10(T a) => default;
        T NaturalLog(T a) => default;

        // Logic
        bool Any(T a, T b) => default;
        bool Equal(T a, T b) => default;
        bool NotEqual(T a, T b) => default;
        bool GreaterThan(T a, T b) => default;
        bool LesserThan(T a, T b) => default;
        bool GreaterEqual(T a, T b) => default;
        bool LesserEqual(T a, T b) => default;
        bool HasFlag(T a, T b) => default;
    }
}