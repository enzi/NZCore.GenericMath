// <copyright project="NZCore.Math" file="FloatExtensions.cs">
// Copyright © 2026 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Mathematics;

namespace NZCore
{
    public static class FloatExtensions
    {
        public static float RoundToOneDecimal(this float value)
        {
            return math.round(value * 10f) / 10f;
        }
    }
}