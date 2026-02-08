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
            "math.pow(a, b)", "a.Pow(b)",
            "math.pow(b, a)", "b.Pow(a)",
            "math.min(a, b)", "a.Min(b)",
            "math.max(a, b)", "a.Max(b)",
            "math.abs(a)", "a.Abs()",
            "math.ceil(a)", "a.Ceil()",
            "math.floor(a)", "a.Floor()",
            "math.round(a)", "a.Round()",
            "math.log10(a)", "a.Log10()",
            "math.log(a)", "a.Log2()",
            "((int)a & (int)b) == (int)b", "false")] // HasFlag not implemented
        public partial struct BigDoubleProcessor : IGenericValueCalculator<BigDouble> { }
    }
}