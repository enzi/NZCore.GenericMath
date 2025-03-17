// <copyright project="NZCore.Math" file="LogicalComparisonHelper.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;

namespace NZCore
{
    public static class LogicalComparisonHelper
    {
        public static bool LogicalComparison(this ConditionLogicValueComparison logicValueComparison, TriggerDataType dataType, GenericUnionValue leftValue, GenericUnionValue rightValue)
        {
            switch (dataType)
            {
                case TriggerDataType.Short:
                    return logicValueComparison.LogicalComparison(leftValue.ShortValue, rightValue.ShortValue);
                case TriggerDataType.UShort:
                    return logicValueComparison.LogicalComparison(leftValue.ShortValue, rightValue.ShortValue);
                case TriggerDataType.Half:
                    return logicValueComparison.LogicalComparison(leftValue.HalfValue, rightValue.HalfValue);
                case TriggerDataType.Float:
                    return logicValueComparison.LogicalComparison(leftValue.FloatValue, rightValue.FloatValue);
                case TriggerDataType.Int:
                    return logicValueComparison.LogicalComparison(leftValue.IntValue, rightValue.IntValue);
                case TriggerDataType.UInt:
                    return logicValueComparison.LogicalComparison(leftValue.UIntValue, rightValue.UIntValue);
                case TriggerDataType.Double:
                    return logicValueComparison.LogicalComparison(leftValue.DoubleValue, rightValue.DoubleValue);
                case TriggerDataType.ULong:
                    return logicValueComparison.LogicalComparison(leftValue.ULongValue, rightValue.ULongValue);
                case TriggerDataType.Long:
                    return logicValueComparison.LogicalComparison(leftValue.LongValue, rightValue.LongValue);
                case TriggerDataType.Byte:
                    return logicValueComparison.LogicalComparison(leftValue.ByteValue, rightValue.ByteValue);
                case TriggerDataType.Bool:
                case TriggerDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For Key {dataType}");
            }
        }
    }
}