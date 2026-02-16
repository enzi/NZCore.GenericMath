// <copyright project="NZCore.Math" file="DefaultProcessors.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using Unity.Mathematics;

namespace NZCore
{
    public class MathMethodReplacement
    {
        public string OldValue { get; }
        public string NewValue { get; }

        public MathMethodReplacement(string oldValue, string newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public class ReplaceMathMethodAttribute : Attribute
    {
        public MathMethodReplacement[] Replacements { get; }

        public ReplaceMathMethodAttribute(params string[] replacements)
        {
            if (replacements.Length % 2 != 0)
                throw new ArgumentException("Replacements must be provided in pairs (oldValue, newValue)");

            Replacements = new MathMethodReplacement[replacements.Length / 2];
            for (int i = 0; i < replacements.Length; i += 2)
            {
                Replacements[i / 2] = new MathMethodReplacement(replacements[i], replacements[i + 1]);
            }
        }
    }
    
    public static partial class GenericMathProcessor
    {
        public partial struct ByteProcessor : IGenericValueCalculator<byte> { }

        public partial struct DoubleProcessor : IGenericValueCalculator<double> { }

        public partial struct FloatProcessor : IGenericValueCalculator<float> { }

        public partial struct HalfProcessor : IGenericValueCalculator<half> { }

        public partial struct IntProcessor : IGenericValueCalculator<int> { }

        public partial struct ShortProcessor : IGenericValueCalculator<short> { }

        public partial struct UShortProcessor : IGenericValueCalculator<ushort> { }

        public partial struct UIntProcessor : IGenericValueCalculator<uint> { }

        public partial struct LongProcessor : IGenericValueCalculator<long> { }

        public partial struct ULongProcessor : IGenericValueCalculator<ulong> { }

        [ReplaceMathMethod(
            "math.pow(a, b)", "BigDouble.Pow(a, (double) b)",
            "math.pow(b, a)", "BigDouble.Pow(b, (double) a)",
            "math.min(a, b)", "BigDouble.Min(a, b)",
            "math.max(a, b)", "BigDouble.Max(a, b)",
            "math.abs(a)", "BigDouble.Abs(a)",
            "math.ceil(a)", "BigDouble.Ceil(a)",
            "math.floor(a)", "BigDouble.Floor(a)",
            "math.round(a)", "BigDouble.Round(a)",
            "math.log10(a)", "BigDouble.Log10(a)",
            "math.log(a)", "BigDouble.Log2(a)",
            "((int)a & (int)b) == (int)b", "false")] // HasFlag not implemented
        public partial struct BigDoubleProcessor : IGenericValueCalculator<BigDouble> { }
    }
}