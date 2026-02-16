// <copyright project="NZCore.Math" file="LogicalComparisonHelper.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;

namespace NZCore
{
    public static class LogicalComparisonHelper
    {
        public static bool LogicalComparison(this ConditionLogicValueComparison logicValueComparison, GenericDataType dataType, GenericUnionValue leftValue, GenericUnionValue rightValue)
        {
            return dataType switch
            {
                GenericDataType.BigDouble => logicValueComparison.LogicalComparison(leftValue.BigDoubleValue, rightValue.BigDoubleValue),
                GenericDataType.Short => logicValueComparison.LogicalComparison(leftValue.ShortValue, rightValue.ShortValue),
                GenericDataType.UShort => logicValueComparison.LogicalComparison(leftValue.UShortValue, rightValue.UShortValue),
                GenericDataType.Half => logicValueComparison.LogicalComparison(leftValue.HalfValue, rightValue.HalfValue),
                GenericDataType.Float => logicValueComparison.LogicalComparison(leftValue.FloatValue, rightValue.FloatValue),
                GenericDataType.Int => logicValueComparison.LogicalComparison(leftValue.IntValue, rightValue.IntValue),
                GenericDataType.UInt => logicValueComparison.LogicalComparison(leftValue.UIntValue, rightValue.UIntValue),
                GenericDataType.Double => logicValueComparison.LogicalComparison(leftValue.DoubleValue, rightValue.DoubleValue),
                GenericDataType.ULong => logicValueComparison.LogicalComparison(leftValue.ULongValue, rightValue.ULongValue),
                GenericDataType.Long => logicValueComparison.LogicalComparison(leftValue.LongValue, rightValue.LongValue),
                GenericDataType.Byte => logicValueComparison.LogicalComparison(leftValue.ByteValue, rightValue.ByteValue),
                GenericDataType.Bool or GenericDataType.None => throw new ArgumentOutOfRangeException($"For Key {dataType}"),
                _ => throw new ArgumentOutOfRangeException($"For Key {dataType}")
            };
        }
    }
}