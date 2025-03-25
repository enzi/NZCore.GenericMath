// <copyright project="NZCore.Math" file="GenericMath.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using Unity.Mathematics;
using UnityEngine;

namespace NZCore
{
    public static partial class GenericMath
    {
        public static unsafe bool ProcessReturnChange<T, TProcessor>(this MathOperator mathOperator, byte* valuePtr, T rightValue)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            var processor = default(TProcessor);
            ref var value = ref *(T*)valuePtr;

            var newValue = mathOperator switch
            {
                MathOperator.Set => rightValue,
                MathOperator.Add => processor.Add(value, rightValue),
                MathOperator.Subtract => processor.Subtract(value, rightValue),
                MathOperator.Multiply => processor.Multiply(value, rightValue),
                MathOperator.Divide => processor.Divide(value, rightValue),
                MathOperator.PowerAtoB => processor.PowerAtoB(value, rightValue),
                MathOperator.PowerBtoA => processor.PowerBtoA(value, rightValue),
                MathOperator.Min => processor.Min(value, rightValue),
                MathOperator.Max => processor.Max(value, rightValue),
                _ => value
            };

            if (newValue.Equals(value))
            {
                return false;
            }

            //Debug.Log($"SET {leftValue} = {newValue}");
            value = newValue;
            return true;

        }
        
        public static bool ProcessReturnChange<T, TProcessor>(this MathOperator mathOperator, ref T leftValue, T rightValue)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            var processor = default(TProcessor);

            var newValue = mathOperator switch
            {
                MathOperator.Set => rightValue,
                MathOperator.Add => processor.Add(leftValue, rightValue),
                MathOperator.Subtract => processor.Subtract(leftValue, rightValue),
                MathOperator.Multiply => processor.Multiply(leftValue, rightValue),
                MathOperator.Divide => processor.Divide(leftValue, rightValue),
                MathOperator.PowerAtoB => processor.PowerAtoB(leftValue, rightValue),
                MathOperator.PowerBtoA => processor.PowerBtoA(leftValue, rightValue),
                MathOperator.Min => processor.Min(leftValue, rightValue),
                MathOperator.Max => processor.Max(leftValue, rightValue),
                _ => leftValue
            };

            if (newValue.Equals(leftValue))
            {
                return false;
            }

            //Debug.Log($"SET {leftValue} = {newValue}");
            leftValue = newValue;
            return true;

        }
        
        public static unsafe bool ProcessWithMinMax<T, TProcessor>(this MathOperator mathOperator, byte* valuePtr, T changeValue, T minValue, T maxValue)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            var processor = default(TProcessor);
            ref var value = ref *(T*)valuePtr;

            var newValue = mathOperator switch
            {
                MathOperator.Set => changeValue,
                MathOperator.Add => processor.Add(value, changeValue),
                MathOperator.Subtract => processor.Subtract(value, changeValue),
                MathOperator.Multiply => processor.Multiply(value, changeValue),
                MathOperator.Divide => processor.Divide(value, changeValue),
                MathOperator.PowerAtoB => processor.PowerAtoB(value, changeValue),
                MathOperator.PowerBtoA => processor.PowerBtoA(value, changeValue),
                _ => value
            };

            newValue = processor.Min(newValue, maxValue);
            newValue = processor.Max(newValue, minValue);

            if (newValue.Equals(value))
            {
                return false;
            }

            //Debug.Log($"SET {value} = {newValue}");
            value = newValue;
            return true;
        }

        public static unsafe void Process<T, TProcessor>(this MathOperator mathOperator, byte* valuePtrToA, byte* valuePtrToB)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            ref T valueA = ref *(T*)valuePtrToA;
            valueA = Process<T, TProcessor>(mathOperator, valueA, valuePtrToB);
        }

        public static unsafe T Process<T, TProcessor>(this MathOperator mathOperator, byte* valuePtrToA, T valueB)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            return Process<T, TProcessor>(mathOperator, *(T*)valuePtrToA, valueB);
        }

        public static unsafe T Process<T, TProcessor>(this MathOperator mathOperator, T valueA, byte* valuePtrToB)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            return Process<T, TProcessor>(mathOperator, valueA, *(T*)valuePtrToB);
        }

        public static T Process<T, TProcessor>(this MathOperator mathOperator, T valueA, T valueB)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            var processor = default(TProcessor);

            return mathOperator switch
            {
                MathOperator.Set => valueB,
                MathOperator.Add => processor.Add(valueA, valueB),
                MathOperator.Subtract => processor.Subtract(valueA, valueB),
                MathOperator.Multiply => processor.Multiply(valueA, valueB),
                MathOperator.Divide => processor.Divide(valueA, valueB),
                MathOperator.PowerAtoB => processor.PowerAtoB(valueA, valueB),
                MathOperator.PowerBtoA => processor.PowerBtoA(valueA, valueB),
                MathOperator.Min => processor.Min(valueA, valueB),
                MathOperator.Max => processor.Max(valueA, valueB),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static void Process(this MathOperator mathOperator, GenericDataType dataType, ref GenericUnionValue leftValue, GenericUnionValue rightValue)
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                    leftValue.ShortValue = mathOperator.ProcessReturnValue(leftValue.ShortValue, rightValue.ShortValue);
                    break;
                case GenericDataType.UShort:
                    leftValue.UShortValue = mathOperator.ProcessReturnValue(leftValue.UShortValue, rightValue.UShortValue);
                    break;
                case GenericDataType.Half:
                    leftValue.HalfValue = mathOperator.ProcessReturnValue(leftValue.HalfValue, rightValue.HalfValue);
                    break;
                case GenericDataType.Float:
                    leftValue.FloatValue = mathOperator.ProcessReturnValue(leftValue.FloatValue, rightValue.FloatValue);
                    break;
                case GenericDataType.Int:
                    leftValue.IntValue = mathOperator.ProcessReturnValue(leftValue.IntValue, rightValue.IntValue);
                    break;
                case GenericDataType.UInt:
                    leftValue.UIntValue = mathOperator.ProcessReturnValue(leftValue.UIntValue, rightValue.UIntValue);
                    break;
                case GenericDataType.Double:
                    leftValue.DoubleValue = mathOperator.ProcessReturnValue(leftValue.DoubleValue, rightValue.DoubleValue);
                    break;
                case GenericDataType.ULong:
                    leftValue.ULongValue = mathOperator.ProcessReturnValue(leftValue.ULongValue, rightValue.ULongValue);
                    break;
                case GenericDataType.Long:
                    leftValue.LongValue = mathOperator.ProcessReturnValue(leftValue.LongValue, rightValue.LongValue);
                    break;
                case GenericDataType.Byte:
                    leftValue.ByteValue = mathOperator.ProcessReturnValue(leftValue.ByteValue, rightValue.ByteValue);
                    break;
                case GenericDataType.Bool:
                    leftValue.BoolValue = mathOperator.ProcessReturnValue(leftValue.ByteValue, rightValue.ByteValue) > 0;
                    break;
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For dataType {dataType}");
            }
        }
        
        public static GenericUnionValue ProcessReturnValue(this MathOperator mathOperator, GenericDataType dataType, GenericUnionValue leftValue, GenericUnionValue rightValue)
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                    return new GenericUnionValue { ShortValue = mathOperator.ProcessReturnValue(leftValue.ShortValue, rightValue.ShortValue) };
                case GenericDataType.UShort:
                    return new GenericUnionValue { UShortValue = mathOperator.ProcessReturnValue(leftValue.UShortValue, rightValue.UShortValue) };
                case GenericDataType.Half:
                    return new GenericUnionValue { HalfValue = mathOperator.ProcessReturnValue(leftValue.HalfValue, rightValue.HalfValue) };
                case GenericDataType.Float:
                    return new GenericUnionValue { FloatValue = mathOperator.ProcessReturnValue(leftValue.FloatValue, rightValue.FloatValue) };
                case GenericDataType.Int:
                    return new GenericUnionValue { IntValue = mathOperator.ProcessReturnValue(leftValue.IntValue, rightValue.IntValue) };
                case GenericDataType.UInt:
                    return new GenericUnionValue { UIntValue = mathOperator.ProcessReturnValue(leftValue.UIntValue, rightValue.UIntValue) };
                case GenericDataType.Double:
                    return new GenericUnionValue { DoubleValue = mathOperator.ProcessReturnValue(leftValue.DoubleValue, rightValue.DoubleValue) };
                case GenericDataType.ULong:
                    return new GenericUnionValue { ULongValue = mathOperator.ProcessReturnValue(leftValue.ULongValue, rightValue.ULongValue) };
                case GenericDataType.Long:
                    return new GenericUnionValue { LongValue = mathOperator.ProcessReturnValue(leftValue.LongValue, rightValue.LongValue) };
                case GenericDataType.Byte:
                    return new GenericUnionValue { ByteValue = mathOperator.ProcessReturnValue(leftValue.ByteValue, rightValue.ByteValue) };
                case GenericDataType.Bool:
                    return new GenericUnionValue { BoolValue = mathOperator.ProcessReturnValue(leftValue.ByteValue, rightValue.ByteValue) > 0 };
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For dataType {dataType}");
            }
        }

        public static GenericUnionValue ProcessMathFunction(this MathFunction mathFunction, GenericDataType dataType, GenericUnionValue value)
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                    return new GenericUnionValue { ShortValue = mathFunction.ProcessMathFunction(value.ShortValue) };
                case GenericDataType.UShort:
                    return new GenericUnionValue { UShortValue = mathFunction.ProcessMathFunction(value.UShortValue) };
                case GenericDataType.Half:
                    return new GenericUnionValue { HalfValue = mathFunction.ProcessMathFunction(value.HalfValue) };
                case GenericDataType.Float:
                    return new GenericUnionValue { FloatValue = mathFunction.ProcessMathFunction(value.FloatValue) };
                case GenericDataType.Int:
                    return new GenericUnionValue { IntValue = mathFunction.ProcessMathFunction(value.IntValue) };
                case GenericDataType.UInt:
                    return new GenericUnionValue { UIntValue = mathFunction.ProcessMathFunction(value.UIntValue) };
                case GenericDataType.Double:
                    return new GenericUnionValue { DoubleValue = mathFunction.ProcessMathFunction(value.DoubleValue) };
                case GenericDataType.ULong:
                    return new GenericUnionValue { ULongValue = mathFunction.ProcessMathFunction(value.ULongValue) };
                case GenericDataType.Long:
                    return new GenericUnionValue { LongValue = mathFunction.ProcessMathFunction(value.LongValue) };
                case GenericDataType.Byte:
                    return new GenericUnionValue { ByteValue = mathFunction.ProcessMathFunction(value.ByteValue) };
                case GenericDataType.Bool:
                    return new GenericUnionValue { BoolValue = mathFunction.ProcessMathFunction(value.ByteValue) > 0 };
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For dataType {dataType}");
            }
        }
        
        public static unsafe void Process(this MathOperator mathOperator, GenericDataType dataType, byte* valuePtr, GenericUnionValue value)
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                {
                    mathOperator.Process(dataType, valuePtr, value.ShortValue);
                    break;
                }
                case GenericDataType.UShort:
                {
                    mathOperator.Process(dataType, valuePtr, value.UShortValue);
                    break;
                }
                case GenericDataType.Half:
                {
                    mathOperator.Process(dataType, valuePtr, value.HalfValue);
                    break;
                }
                case GenericDataType.Float:
                {
                    mathOperator.Process(dataType, valuePtr, value.FloatValue);
                    break;
                }
                case GenericDataType.Int:
                {
                    mathOperator.Process(dataType, valuePtr, value.IntValue);
                    break;
                }
                case GenericDataType.UInt:
                {
                    mathOperator.Process(dataType, valuePtr, value.UIntValue);
                    break;
                }
                case GenericDataType.Double:
                {
                    mathOperator.Process(dataType, valuePtr, value.DoubleValue);
                    break;
                }
                case GenericDataType.ULong:
                {
                    mathOperator.Process(dataType, valuePtr, value.ULongValue);
                    break;
                }
                case GenericDataType.Long:
                {
                    mathOperator.Process(dataType, valuePtr, value.LongValue);
                    break;
                }
                case GenericDataType.Byte:
                case GenericDataType.Bool:
                {
                    mathOperator.Process(dataType, valuePtr, value.ByteValue);
                    break;
                }
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For dataType {dataType}");
            }
        }
        
        public static unsafe bool LogicalComparison(this ConditionLogicValueComparison logicValueComparison, GenericDataType dataType, byte* valueA, GenericUnionValue valueB)
        {
            switch (dataType)
            {
                case GenericDataType.Byte:
                {
                    var val = *(byte*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.ByteValue);
                }
                case GenericDataType.Double:
                {
                    var val = *(double*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.DoubleValue);
                }
                case GenericDataType.Float:
                {
                    var val = *(float*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.FloatValue);
                }
                case GenericDataType.Half:
                {
                    var val = *(half*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.HalfValue);
                }
                case GenericDataType.Int:
                {
                    var val = *(int*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.IntValue);
                }
                case GenericDataType.Short:
                {
                    var val = *(short*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.ShortValue);
                }
                case GenericDataType.UShort:
                {
                    var val = *(ushort*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.UShortValue);
                }
                case GenericDataType.UInt:
                {
                    var val = *(uint*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.UIntValue);
                }
                case GenericDataType.Long:
                {
                    var val = *(long*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.LongValue);
                }
                case GenericDataType.ULong:
                {
                    var val = *(ulong*)valueA;
                    return logicValueComparison.LogicalComparison(val, valueB.ULongValue);
                }
            }

            return false;
        }

        public static unsafe bool MultiplyBaseValue(GenericDataType dataType, byte* valuePtr, byte* baseValuePtr, int multiplier)
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                {
                    ref var value = ref *(short*)valuePtr;
                    ref var baseValue = ref *(short*)baseValuePtr;
                    var newValue = (short)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.UShort:
                {
                    ref var value = ref *(ushort*)valuePtr;
                    ref var baseValue = ref *(ushort*)baseValuePtr;
                    var newValue = (ushort)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.Half:
                {
                    ref var value = ref *(half*)valuePtr;
                    ref var baseValue = ref *(half*)baseValuePtr;
                    var newValue = (half)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.Float:
                {
                    ref float value = ref *(float*)valuePtr;
                    ref float baseValue = ref *(float*)baseValuePtr;
                    var newValue = baseValue * multiplier;

                    if (Mathf.Approximately(newValue, value))
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.Int:
                {
                    ref var value = ref *(int*)valuePtr;
                    ref var baseValue = ref *(int*)baseValuePtr;
                    var newValue = (int)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.UInt:
                {
                    ref var value = ref *(half*)valuePtr;
                    ref var baseValue = ref *(half*)baseValuePtr;
                    var newValue = (half)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.Double:
                {
                    ref var value = ref *(double*)valuePtr;
                    ref var baseValue = ref *(double*)baseValuePtr;
                    var newValue = (double)(baseValue * multiplier);

                    if ((Math.Abs(newValue - value) < double.Epsilon))
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.Long:
                {
                    ref var value = ref *(long*)valuePtr;
                    ref var baseValue = ref *(long*)baseValuePtr;
                    var newValue = (long)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.ULong:
                {
                    ref var value = ref *(ulong*)valuePtr;
                    ref var baseValue = ref *(ulong*)baseValuePtr;
                    var newValue = (ulong)(baseValue * (ulong) multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
                case GenericDataType.Byte:
                {
                    ref var value = ref *(byte*)valuePtr;
                    ref var baseValue = ref *(byte*)baseValuePtr;
                    var newValue = (byte)(baseValue * multiplier);

                    if (newValue == value)
                    {
                        return false;
                    }

                    value = newValue;
                    return true;
                }
            }

            return false;
        }

        // TODO wait until .NET6 arrives in Unity :)
        // public interface IEqualityOperators<TSelf, TOther, TResult> where TSelf : IEqualityOperators<TSelf, TOther, TResult>?
        // {
        //     static abstract TResult operator ==(TSelf? left, TOther? right);
        //     static abstract TResult operator !=(TSelf? left, TOther? right);
        // }
        //
        // public interface IComparisonOperators<TSelf, TOther, TResult> : IEqualityOperators<TSelf, TOther, TResult> where TSelf : IComparisonOperators<TSelf, TOther, TResult>?
        // {
        //     static abstract TResult operator <(TSelf left, TOther right);
        //     static abstract TResult operator >(TSelf left, TOther right);
        //     static abstract TResult operator <=(TSelf left, TOther right);
        //     static abstract TResult operator >=(TSelf left, TOther right);
        // }

        // public interface INumber<TSelf>
        //     : IComparable,
        //         IComparable<TSelf>,
        //         IComparisonOperators<TSelf, TSelf, bool>,
        //         IModulusOperators<TSelf, TSelf, TSelf>,
        //         INumberBase<TSelf>
        //     where TSelf : INumber<TSelf>?
        // {
        //     
        // }
    }
}