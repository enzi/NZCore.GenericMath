// <copyright project="NZCore" file="GenericDataTypeExtensions.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace NZCore
{
    public static class GenericDataTypeExtensions
    {
        public static int GetByteSize(this GenericDataType dataType)
        {
            return dataType switch
            {
                GenericDataType.Byte or GenericDataType.Bool => 1,
                GenericDataType.Short or GenericDataType.UShort or GenericDataType.Half => 2,
                GenericDataType.Float or GenericDataType.Int or GenericDataType.UInt => 4,
                GenericDataType.Double or GenericDataType.Long or GenericDataType.ULong => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(dataType), $"{dataType}", $"{dataType} has not been implemented!")
            };
        }

        public static void AddGenericValue<T>(this ref DynamicBuffer<T> buffer, GenericDataType dataType, GenericUnionValue value)
            where T : unmanaged
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                    buffer.AddToByteBuffer(value.ShortValue);
                    break;
                case GenericDataType.UShort:
                    buffer.AddToByteBuffer(value.UShortValue);
                    break;
                case GenericDataType.Half:
                    buffer.AddToByteBuffer(value.HalfValue);
                    break;
                case GenericDataType.Float:
                    buffer.AddToByteBuffer(value.FloatValue);
                    break;
                case GenericDataType.Int:
                    buffer.AddToByteBuffer(value.IntValue);
                    break;
                case GenericDataType.UInt:
                    buffer.AddToByteBuffer(value.UIntValue);
                    break;
                case GenericDataType.Double:
                    buffer.AddToByteBuffer(value.DoubleValue);
                    break;
                case GenericDataType.Long:
                    buffer.AddToByteBuffer(value.LongValue);
                    break;
                case GenericDataType.ULong:
                    buffer.AddToByteBuffer(value.ULongValue);
                    break;
                case GenericDataType.Byte:
                    buffer.AddToByteBuffer(value.ByteValue);
                    break;
                case GenericDataType.Bool:
                    buffer.AddToByteBuffer(value.BoolValue);
                    break;
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void AddGenericValue<T>(this ref DynamicBuffer<T> buffer, GenericDataType dataType, double value)
            where T : unmanaged
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var typeSize = UnsafeUtility.SizeOf<T>();
            Assert.AreEqual(typeSize, 1, $"AddStat -> {nameof(T)} needs to be size 1!");
#endif
            
            switch (dataType)
            {
                case GenericDataType.Bool:
                case GenericDataType.Byte:
                {
                    buffer.AddToByteBuffer((byte)value);
                    break;
                }
                case GenericDataType.Short:
                {
                    buffer.AddToByteBuffer((short)value);
                    break;
                }
                case GenericDataType.UShort:
                {
                    buffer.AddToByteBuffer((ushort)value);
                    break;
                }
                case GenericDataType.Half:
                {
                    buffer.AddToByteBuffer((half)value);
                    break;
                }
                case GenericDataType.Float:
                {
                    buffer.AddToByteBuffer((float)value);
                    //Debug.Log($"set {statType} to {value}");
                    break;
                }
                case GenericDataType.Int:
                {
                    buffer.AddToByteBuffer((int)value);
                    break;
                }
                case GenericDataType.UInt:
                {
                    buffer.AddToByteBuffer((uint)value);
                    break;
                }
                case GenericDataType.Double:
                {
                    buffer.AddToByteBuffer((double)value);
                    //Debug.Log($"set {statType} to {value}");
                    break;
                }
                case GenericDataType.Long:
                {
                    buffer.AddToByteBuffer((long)value);
                    break;
                }
                case GenericDataType.ULong:
                {
                    buffer.AddToByteBuffer((ulong)value);
                    break;
                }
                case GenericDataType.None:
                    //Debug.Log($"{statType} is none!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        public static byte[] GetByteArray(this GenericDataType dataType, double value)
        {
            switch (dataType)
            {
                case GenericDataType.None:
                    break;
                case GenericDataType.Bool:
                {
                    return new [] { value != 0 ? (byte)1 : (byte)0 };
                }
                case GenericDataType.Byte:
                {
                    return new[] { (byte) value };
                }
                case GenericDataType.Short:
                {
                    short tmpValue = (short)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.UShort:
                {
                    ushort tmpValue = (ushort)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.Half:
                {
                    half tmpValue = (half)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.Float:
                {
                    float tmpValue = (float)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.Int:
                {
                    int tmpValue = (int)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.UInt:
                {
                    uint tmpValue = (uint)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.Double:
                {
                    double tmpValue = (double)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.Long:
                {
                    long tmpValue = (long)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                case GenericDataType.ULong:
                {
                    ulong tmpValue = (ulong)value;
                    return ByteHelper.GetBytes(tmpValue);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}