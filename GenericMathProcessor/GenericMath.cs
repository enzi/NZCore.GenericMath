// <copyright project="NZCore.Math" file="GenericMath.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;

namespace NZCore
{
    public static partial class GenericMath
    {
        public static unsafe bool ProcessValuesWithMinMax<T, TProcessor>(byte* valuePtr, MathOperator mathOperator, T changeValue, T minValue, T maxValue)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            var processor = default(TProcessor);
            ref var value = ref *(T*)valuePtr;

            switch (mathOperator)
            {
                case MathOperator.Set:
                {
                    var newValue = changeValue;
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        //Debug.Log($"SET {value} = {newValue}");
                        value = newValue;
                        return true;
                    }

                    break;
                }
                case MathOperator.Add:
                {
                    var newValue = processor.Add(value, changeValue);
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        value = newValue;
                        return true;
                    }

                    break;
                }
                case MathOperator.Subtract:
                {
                    var newValue = processor.Subtract(value, changeValue);
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        value = newValue;
                        return true;
                    }

                    break;
                }
                case MathOperator.Multiply:
                {
                    var newValue = processor.Multiply(value, changeValue);
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        value = newValue;
                        return true;
                    }

                    break;
                }
                case MathOperator.Divide:
                {
                    var newValue = processor.Divide(value, changeValue);
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        value = newValue;
                        return true;
                    }

                    break;
                }
                case MathOperator.PowerAtoB:
                {
                    var newValue = processor.PowerAtoB(value, changeValue);
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        value = newValue;
                        return true;
                    }

                    break;
                }
                case MathOperator.PowerBtoA:
                {
                    var newValue = processor.PowerBtoA(value, changeValue);
                    newValue = processor.Min(newValue, maxValue);
                    newValue = processor.Max(newValue, minValue);

                    if (!newValue.Equals(value))
                    {
                        value = newValue;
                        return true;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public static unsafe void ProcessValues<T, TProcessor>(byte* valuePtrToA, byte* valuePtrToB, MathOperator mathOperator)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            ref T valueA = ref *(T*)valuePtrToA;
            valueA = ProcessValues<T, TProcessor>(valueA, valuePtrToB, mathOperator);
        }

        public static unsafe T ProcessValues<T, TProcessor>(byte* valuePtrToA, T valueB, MathOperator mathOperator)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            return ProcessValues<T, TProcessor>(*(T*)valuePtrToA, valueB, mathOperator);
        }

        public static unsafe T ProcessValues<T, TProcessor>(T valueA, byte* valuePtrToB, MathOperator mathOperator)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
        {
            return ProcessValues<T, TProcessor>(valueA, *(T*)valuePtrToB, mathOperator);
        }

        public static T ProcessValues<T, TProcessor>(T valueA, T valueB, MathOperator mathOperator)
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
        
        public static GenericUnionValue ProcessValues(TriggerDataType dataType, GenericUnionValue leftValue, GenericUnionValue rightValue, MathOperator mathOperator)
        {
            switch (dataType)
            {
                case TriggerDataType.Short:
                    return new GenericUnionValue { ShortValue = ProcessValues(leftValue.ShortValue, rightValue.ShortValue, mathOperator) };
                case TriggerDataType.UShort:
                    return new GenericUnionValue { UShortValue = ProcessValues(leftValue.UShortValue, rightValue.UShortValue, mathOperator) };
                case TriggerDataType.Half:
                    return new GenericUnionValue { HalfValue = ProcessValues(leftValue.HalfValue, rightValue.HalfValue, mathOperator) };
                case TriggerDataType.Float:
                    return new GenericUnionValue { FloatValue = ProcessValues(leftValue.FloatValue, rightValue.FloatValue, mathOperator) };
                case TriggerDataType.Int:
                    return new GenericUnionValue { IntValue = ProcessValues(leftValue.IntValue, rightValue.IntValue, mathOperator) };
                case TriggerDataType.UInt:
                    return new GenericUnionValue { UIntValue = ProcessValues(leftValue.UIntValue, rightValue.UIntValue, mathOperator) };
                case TriggerDataType.Double:
                    return new GenericUnionValue { DoubleValue = ProcessValues(leftValue.DoubleValue, rightValue.DoubleValue, mathOperator) };
                case TriggerDataType.ULong:
                    return new GenericUnionValue { ULongValue = ProcessValues(leftValue.ULongValue, rightValue.ULongValue, mathOperator) };
                case TriggerDataType.Long:
                    return new GenericUnionValue { LongValue = ProcessValues(leftValue.LongValue, rightValue.LongValue, mathOperator) };
                case TriggerDataType.Byte:
                    return new GenericUnionValue { ByteValue = ProcessValues(leftValue.ByteValue, rightValue.ByteValue, mathOperator) };
                case TriggerDataType.Bool:
                    return new GenericUnionValue { BoolValue = ProcessValues(leftValue.ByteValue, rightValue.ByteValue, mathOperator) > 0 };
                case TriggerDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For dataType {dataType}");
            }
        }

        public static GenericUnionValue ProcessMathFunction(GenericDataType dataType, MathFunction mathFunction, GenericUnionValue value)
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
        
        public static unsafe void Process(GenericDataType dataType, MathOperator mathOperator, byte* valuePtr, GenericUnionValue value)
        {
            switch (dataType)
            {
                case GenericDataType.Short:
                {
                    Process(dataType, mathOperator, valuePtr, value.ShortValue);
                    break;
                }
                case GenericDataType.UShort:
                {
                    Process(dataType, mathOperator, valuePtr, value.UShortValue);
                    break;
                }
                case GenericDataType.Half:
                {
                    Process(dataType, mathOperator, valuePtr, value.HalfValue);
                    break;
                }
                case GenericDataType.Float:
                {
                    Process(dataType, mathOperator, valuePtr, value.FloatValue);
                    break;
                }
                case GenericDataType.Int:
                {
                    Process(dataType, mathOperator, valuePtr, value.IntValue);
                    break;
                }
                case GenericDataType.UInt:
                {
                    Process(dataType, mathOperator, valuePtr, value.UIntValue);
                    break;
                }
                case GenericDataType.Double:
                {
                    Process(dataType, mathOperator, valuePtr, value.DoubleValue);
                    break;
                }
                case GenericDataType.ULong:
                {
                    Process(dataType, mathOperator, valuePtr, value.ULongValue);
                    break;
                }
                case GenericDataType.Long:
                {
                    Process(dataType, mathOperator, valuePtr, value.LongValue);
                    break;
                }
                case GenericDataType.Byte:
                case GenericDataType.Bool:
                {
                    Process(dataType, mathOperator, valuePtr, value.ByteValue);
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