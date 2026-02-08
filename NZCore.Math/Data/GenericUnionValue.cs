// <copyright project="NZCore" file="GenericUnionValue.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace NZCore
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct GenericUnionValue
    {
#if BIGDOUBLE
        [FieldOffset(0)] public BigDouble BigDoubleValue;
#endif
        // 64 bit
        [FieldOffset(0)] public double DoubleValue;
        [FieldOffset(0)] public long LongValue;
        [FieldOffset(0)] public ulong ULongValue;

        // 32 bit
        [FieldOffset(0)] public float FloatValue;
        [FieldOffset(0)] public int IntValue;
        [FieldOffset(0)] public uint UIntValue;

        // 16 bit
        [FieldOffset(0)] public half HalfValue;
        [FieldOffset(0)] public short ShortValue;
        [FieldOffset(0)] public ushort UShortValue;

        // 8 bit
        [FieldOffset(0)] public byte ByteValue;
        [FieldOffset(0)] public bool BoolValue;

#if BIGDOUBLE
        public static GenericUnionValue Create(BigDouble val)
        {
            return new GenericUnionValue()
            {
                BigDoubleValue = val
            };
        }
#endif
        
        public static GenericUnionValue Create(double val)
        {
            return new GenericUnionValue()
            {
                DoubleValue = val
            };
        }
        
        public static GenericUnionValue Create(long val)
        {
            return new GenericUnionValue()
            {
                LongValue = val
            };
        }
        
        public static GenericUnionValue Create(ulong val)
        {
            return new GenericUnionValue()
            {
                ULongValue = val
            };
        }
        
        public static GenericUnionValue Create(float val)
        {
            return new GenericUnionValue()
            {
                FloatValue = val
            };
        }
        
        public static GenericUnionValue Create(int val)
        {
            return new GenericUnionValue()
            {
                IntValue = val
            };
        }
        
        public static GenericUnionValue Create(uint val)
        {
            return new GenericUnionValue()
            {
                UIntValue = val
            };
        }
        
        public static GenericUnionValue Create(half val)
        {
            return new GenericUnionValue()
            {
                HalfValue = val
            };
        }
        
        public static GenericUnionValue Create(short val)
        {
            return new GenericUnionValue()
            {
                ShortValue = val
            };
        }
        
        public static GenericUnionValue Create(ushort val)
        {
            return new GenericUnionValue()
            {
                UShortValue = val
            };
        }
        
        public static GenericUnionValue Create(byte val)
        {
            return new GenericUnionValue()
            {
                ByteValue = val
            };
        }
        
        public static GenericUnionValue Create(bool val)
        {
            return new GenericUnionValue()
            {
                BoolValue = val
            };
        }
        
        public static unsafe GenericUnionValue Create(GenericDataType dataType, byte* valuePtr)
        {
            switch (dataType)
            {
#if BIGDOUBLE
                case GenericDataType.BigDouble:
                    return new GenericUnionValue() { BigDoubleValue = *(BigDouble*)valuePtr };
#endif
                case GenericDataType.Short:
                    return new GenericUnionValue() { ShortValue = *(short*)valuePtr };
                case GenericDataType.UShort:
                    return new GenericUnionValue() { UShortValue = *(ushort*)valuePtr };
                case GenericDataType.Half:
                    return new GenericUnionValue() { HalfValue = *(half*)valuePtr };
                case GenericDataType.Float:
                    return new GenericUnionValue() { FloatValue = *(float*)valuePtr };
                case GenericDataType.Int:
                    return new GenericUnionValue() { IntValue = *(int*)valuePtr };
                case GenericDataType.UInt:
                    return new GenericUnionValue() { UIntValue = *(uint*)valuePtr };
                case GenericDataType.Double:
                    return new GenericUnionValue() { DoubleValue = *(double*)valuePtr };
                case GenericDataType.Long:
                    return new GenericUnionValue() { LongValue = *(long*)valuePtr };
                case GenericDataType.ULong:
                    return new GenericUnionValue() { ULongValue = *(ulong*)valuePtr };
                case GenericDataType.Byte:
                    return new GenericUnionValue() { ByteValue = *valuePtr };
                case GenericDataType.Bool:
                    return new GenericUnionValue() { BoolValue = *valuePtr > 0 };
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        public static GenericUnionValue Create(GenericDataType dataType, double value)
        {
            switch (dataType)
            {
#if BIGDOUBLE
                case GenericDataType.BigDouble:
                    return new GenericUnionValue() { BigDoubleValue = new BigDouble(value) };
#endif
                case GenericDataType.Short:
                    return new GenericUnionValue() { ShortValue = (short)value };
                case GenericDataType.UShort:
                    return new GenericUnionValue() { UShortValue = (ushort)value };
                case GenericDataType.Half:
                    return new GenericUnionValue() { HalfValue = (half)value };
                case GenericDataType.Float:
                    return new GenericUnionValue() { FloatValue = (float)value };
                case GenericDataType.Int:
                    return new GenericUnionValue() { IntValue = (int)value };
                case GenericDataType.UInt:
                    return new GenericUnionValue() { UIntValue = (uint)value };
                case GenericDataType.Double:
                    return new GenericUnionValue() { DoubleValue = (double)value };
                case GenericDataType.Long:
                    return new GenericUnionValue() { LongValue = (long)value };
                case GenericDataType.ULong:
                    return new GenericUnionValue() { ULongValue = (ulong)value };
                case GenericDataType.Byte:
                    return new GenericUnionValue() { ByteValue = (byte)value };
                case GenericDataType.Bool:
                    return new GenericUnionValue() { BoolValue = value > 0 };
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
    }
}