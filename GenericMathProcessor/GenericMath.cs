// <copyright project="NZCore.Math" file="GenericMath.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;

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
        
        public static GenericUnionValue ProcessReturnValue(this MathOperator mathOperator, TriggerDataType dataType, GenericUnionValue leftValue, GenericUnionValue rightValue)
        {
            switch (dataType)
            {
                case TriggerDataType.Short:
                    return new GenericUnionValue { ShortValue = mathOperator.ProcessReturnValue(leftValue.ShortValue, rightValue.ShortValue) };
                case TriggerDataType.UShort:
                    return new GenericUnionValue { UShortValue = mathOperator.ProcessReturnValue(leftValue.UShortValue, rightValue.UShortValue) };
                case TriggerDataType.Half:
                    return new GenericUnionValue { HalfValue = mathOperator.ProcessReturnValue(leftValue.HalfValue, rightValue.HalfValue) };
                case TriggerDataType.Float:
                    return new GenericUnionValue { FloatValue = mathOperator.ProcessReturnValue(leftValue.FloatValue, rightValue.FloatValue) };
                case TriggerDataType.Int:
                    return new GenericUnionValue { IntValue = mathOperator.ProcessReturnValue(leftValue.IntValue, rightValue.IntValue) };
                case TriggerDataType.UInt:
                    return new GenericUnionValue { UIntValue = mathOperator.ProcessReturnValue(leftValue.UIntValue, rightValue.UIntValue) };
                case TriggerDataType.Double:
                    return new GenericUnionValue { DoubleValue = mathOperator.ProcessReturnValue(leftValue.DoubleValue, rightValue.DoubleValue) };
                case TriggerDataType.ULong:
                    return new GenericUnionValue { ULongValue = mathOperator.ProcessReturnValue(leftValue.ULongValue, rightValue.ULongValue) };
                case TriggerDataType.Long:
                    return new GenericUnionValue { LongValue = mathOperator.ProcessReturnValue(leftValue.LongValue, rightValue.LongValue) };
                case TriggerDataType.Byte:
                    return new GenericUnionValue { ByteValue = mathOperator.ProcessReturnValue(leftValue.ByteValue, rightValue.ByteValue) };
                case TriggerDataType.Bool:
                    return new GenericUnionValue { BoolValue = mathOperator.ProcessReturnValue(leftValue.ByteValue, rightValue.ByteValue) > 0 };
                case TriggerDataType.None:
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