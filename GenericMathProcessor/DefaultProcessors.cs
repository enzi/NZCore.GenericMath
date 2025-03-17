// <copyright project="NZCore.Math" file="DefaultProcessors.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Mathematics;

namespace NZCore
{
    public static partial class GenericMathProcessor
    {
        public partial struct ByteProcessor : IGenericValueCalculator<byte>
        {
        }

        public partial struct DoubleProcessor : IGenericValueCalculator<double>
        {
        }

        public partial struct FloatProcessor : IGenericValueCalculator<float>
        {
        }

        public partial struct HalfProcessor : IGenericValueCalculator<half>
        {
        }

        public partial struct IntProcessor : IGenericValueCalculator<int>
        {
        }

        public partial struct ShortProcessor : IGenericValueCalculator<short>
        {
        }
        
        public partial struct UShortProcessor : IGenericValueCalculator<ushort>
        {
        }

        public partial struct UIntProcessor : IGenericValueCalculator<uint>
        {
        }

        public partial struct LongProcessor : IGenericValueCalculator<long>
        {
        }

        public partial struct ULongProcessor : IGenericValueCalculator<ulong>
        {
        }
    }
}