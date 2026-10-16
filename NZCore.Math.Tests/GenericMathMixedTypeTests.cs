using NUnit.Framework;

namespace NZCore.Math.Tests
{
    // engines that combine a typed value with a stat of a different type behind a pointer
    [TestFixture]
    public unsafe class GenericMathMixedTypeTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            BigDouble.PowersOf10.Init();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            BigDouble.PowersOf10.Dispose();
        }

        // element modifiers scale the rolled double amount by a stat of any type, the result must keep the amount's precision
        [TestCase(GenericDataType.Int)]
        [TestCase(GenericDataType.Float)]
        [TestCase(GenericDataType.Double)]
        public void ProcessReturnValue_ValueAndStatPtr_ComputesInTheValuesType(GenericDataType statType)
        {
            var stat = GenericUnionValue.Create(statType, 2.0);

            var result = MathOperator.Multiply.ProcessReturnValue(12.7, statType, (byte*)&stat);

            Assert.AreEqual(25.4, result);
        }

        // with BIGDOUBLE the rolled amount is a BigDouble, an int stat must not convert it to int (out of range turns it into 0)
        [Test]
        public void ProcessReturnValue_BigDoubleValueAndIntStat_KeepsRange()
        {
            var stat = 2;

            var result = MathOperator.Multiply.ProcessReturnValue(new BigDouble(1.5, 300), GenericDataType.Int, (byte*)&stat);

            Assert.AreEqual(3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(300L, result.Exponent);
        }

#if BIGDOUBLE
        // a BigDouble stat combined with a BigDouble value must not round trip through double
        [Test]
        public void ProcessReturnValue_BigDoubleStatAndValue_KeepsRange()
        {
            var stat = new BigDouble(1.5, 400);

            var result = MathOperator.Multiply.ProcessReturnValue(GenericDataType.BigDouble, (byte*)&stat, new BigDouble(2.0));

            Assert.AreEqual(3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(400L, result.Exponent);
        }
#endif
    }
}