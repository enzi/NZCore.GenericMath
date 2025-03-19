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
            switch (dataType)
            {
                case GenericDataType.Short:
                    return logicValueComparison.LogicalComparison(leftValue.ShortValue, rightValue.ShortValue);
                case GenericDataType.UShort:
                    return logicValueComparison.LogicalComparison(leftValue.UShortValue, rightValue.UShortValue);
                case GenericDataType.Half:
                    return logicValueComparison.LogicalComparison(leftValue.HalfValue, rightValue.HalfValue);
                case GenericDataType.Float:
                    return logicValueComparison.LogicalComparison(leftValue.FloatValue, rightValue.FloatValue);
                case GenericDataType.Int:
                    return logicValueComparison.LogicalComparison(leftValue.IntValue, rightValue.IntValue);
                case GenericDataType.UInt:
                    return logicValueComparison.LogicalComparison(leftValue.UIntValue, rightValue.UIntValue);
                case GenericDataType.Double:
                    return logicValueComparison.LogicalComparison(leftValue.DoubleValue, rightValue.DoubleValue);
                case GenericDataType.ULong:
                    return logicValueComparison.LogicalComparison(leftValue.ULongValue, rightValue.ULongValue);
                case GenericDataType.Long:
                    return logicValueComparison.LogicalComparison(leftValue.LongValue, rightValue.LongValue);
                case GenericDataType.Byte:
                    return logicValueComparison.LogicalComparison(leftValue.ByteValue, rightValue.ByteValue);
                case GenericDataType.Bool:
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For Key {dataType}");
            }
        }
    }
}