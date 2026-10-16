using System;
using NUnit.Framework;

namespace NZCore.Math.Tests
{
    // Bool values compute through GenericMathProcessor.BoolProcessor: true behaves like 1, false like 0, results stay true/false
    [TestFixture]
    public unsafe class BoolProcessorTests
    {
        // byte math wrapped 0 - 1 to 255, which read back as true. Removing a flag that isn't set must leave it unset
        [Test]
        public void Subtract_FalseMinusTrue_IsFalse()
        {
            var stat = false;

            MathOperator.Subtract.Process(GenericDataType.Bool, (byte*)&stat, true);

            Assert.IsFalse(stat);
        }

        // byte math divided by zero here
        [Test]
        public void Divide_ByFalse_KeepsTheValue()
        {
            var stat = true;

            MathOperator.Divide.Process(GenericDataType.Bool, (byte*)&stat, false);

            Assert.IsTrue(stat);
        }

        // Cast had no Bool cases, so a number cast to Bool kept its low byte: 1.0f and 256 read as false
        [TestCase(GenericDataType.Float, 1.0)]
        [TestCase(GenericDataType.Double, 1.0)]
        [TestCase(GenericDataType.Int, 256)]
        [TestCase(GenericDataType.Int, -1)]
        public void Cast_NonZeroNumberToBool_IsTrue(GenericDataType from, double value)
        {
            var genericValue = GenericUnionValue.Create(from, value);

            genericValue.Cast(from, GenericDataType.Bool);

            Assert.AreEqual(1, genericValue.ByteValue);
        }

        // Bool memory isn't guaranteed to hold 0/1, any non-zero byte counts as true and converts to 1
        [Test]
        public void NonZeroByteInBoolMemory_ReadsAsOne()
        {
            byte stat = 2;

            var result = MathOperator.Add.ProcessReturnValue(5f, GenericDataType.Bool, &stat);

            Assert.AreEqual(6f, result);
        }

        // no required flag passes, like (a & 0) == 0 for numbers
        [Test]
        public void HasFlag_IsTrueUnlessTheFlagIsMissing()
        {
            Assert.IsTrue(ConditionLogicValueComparison.HasFlag.LogicalComparison(false, false));
            Assert.IsTrue(ConditionLogicValueComparison.HasFlag.LogicalComparison(true, true));
            Assert.IsFalse(ConditionLogicValueComparison.HasFlag.LogicalComparison(false, true));
        }

        // typed loops reach Bool through Dispatch instead of a separate runtime-typed path
        [Test]
        public void Dispatch_InvokesBool()
        {
            var action = new RecordTypeAction();

            Assert.IsTrue(GenericMath.Dispatch(GenericDataType.Bool, ref action));
            Assert.AreEqual(typeof(bool), action.Type);
        }

        private struct RecordTypeAction : IGenericMathAction
        {
            public Type Type;

            public void Invoke<T, TProcessor>()
                where T : unmanaged, IEquatable<T>
                where TProcessor : struct, IGenericValueCalculator<T>
            {
                Type = typeof(T);
            }
        }
    }
}