// <copyright project="NZCore" file="GenericDataTypeExtensions.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

        public static void PrintGenericValue(this GenericUnionValue value, GenericDataType dataType)
        {
            switch (dataType)
            {
                case GenericDataType.Half:
                    Debug.Log($"{value.HalfValue}");
                    break;
                case GenericDataType.Short:
                    Debug.Log($"{value.ShortValue}");
                    break;
                case GenericDataType.UShort:
                    Debug.Log($"{value.UShortValue}");
                    break;
                case GenericDataType.Float:
                    Debug.Log($"{value.FloatValue}");
                    break;
                case GenericDataType.Int:
                    Debug.Log($"{value.IntValue}");
                    break;
                case GenericDataType.UInt:
                    Debug.Log($"{value.UIntValue}");
                    break;
                case GenericDataType.Double:
                    Debug.Log($"{value.DoubleValue}");
                    break;
                case GenericDataType.Long:
                    Debug.Log($"{value.LongValue}");
                    break;
                case GenericDataType.ULong:
                    Debug.Log($"{value.ULongValue}");
                    break;
                case GenericDataType.Byte:
                    Debug.Log($"{value.ByteValue}");
                    break;
                case GenericDataType.Bool:
                    Debug.Log($"{value.BoolValue}");
                    break;
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        public static void PrintGenericValue(this GenericUnionValue value, GenericDataType dataType, FixedString128Bytes msg)
        {
            switch (dataType)
            {
                case GenericDataType.Half:
                    Debug.Log($"{msg} {value.HalfValue}");
                    break;
                case GenericDataType.Short:
                    Debug.Log($"{msg} {value.ShortValue}");
                    break;
                case GenericDataType.UShort:
                    Debug.Log($"{msg} {value.UShortValue}");
                    break;
                case GenericDataType.Float:
                    Debug.Log($"{msg} {value.FloatValue}");
                    break;
                case GenericDataType.Int:
                    Debug.Log($"{msg} {value.IntValue}");
                    break;
                case GenericDataType.UInt:
                    Debug.Log($"{msg} {value.UIntValue}");
                    break;
                case GenericDataType.Double:
                    Debug.Log($"{msg} {value.DoubleValue}");
                    break;
                case GenericDataType.Long:
                    Debug.Log($"{msg} {value.LongValue}");
                    break;
                case GenericDataType.ULong:
                    Debug.Log($"{msg} {value.ULongValue}");
                    break;
                case GenericDataType.Byte:
                    Debug.Log($"{msg} {value.ByteValue}");
                    break;
                case GenericDataType.Bool:
                    Debug.Log($"{msg} {value.BoolValue}");
                    break;
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        public static string ToString(this GenericUnionValue value, GenericDataType dataType)
        {
            switch (dataType)
            {
                case GenericDataType.Half:
                    return $"{value.HalfValue}";
                case GenericDataType.Short:
                    return$"{value.ShortValue}";
                case GenericDataType.UShort:
                    return$"{value.UShortValue}";
                case GenericDataType.Float:
                    return$"{value.FloatValue}";
                case GenericDataType.Int:
                    return$"{value.IntValue}";
                case GenericDataType.UInt:
                    return$"{value.UIntValue}";
                case GenericDataType.Double:
                    return$"{value.DoubleValue}";
                case GenericDataType.Long:
                    return$"{value.LongValue}";
                case GenericDataType.ULong:
                    return$"{value.ULongValue}";
                case GenericDataType.Byte:
                    return$"{value.ByteValue}";
                case GenericDataType.Bool:
                    return$"{value.BoolValue}";
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
    }
}