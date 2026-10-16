// <copyright project="NZCore.Math" file="MathExpressionTests.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace NZCore.Math.Tests
{
    [TestFixture]
    public class MathExpressionTests
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

        // ---- test resolvers: payload = variable index, single kind (0) ----

        private struct FloatResolver : IVariableResolver<float>
        {
            public float V0, V1, V2;
            public float Resolve(byte kind, int payload) => payload switch { 0 => V0, 1 => V1, 2 => V2, _ => 0f };
        }

        private struct IntResolver : IVariableResolver<int>
        {
            public int V0, V1, V2;
            public int Resolve(byte kind, int payload) => payload switch { 0 => V0, 1 => V1, 2 => V2, _ => 0 };
        }

        private struct BigDoubleResolver : IVariableResolver<BigDouble>
        {
            public BigDouble V0, V1, V2;
            public BigDouble Resolve(byte kind, int payload) => payload switch { 0 => V0, 1 => V1, 2 => V2, _ => new BigDouble(0) };
        }

        // maps x->0, a->1, b->2; throws on anything else (proves unknown vars fail loud)
        private static (byte kind, int payload) ResolveName(string name) => name switch
        {
            "x" => ((byte)0, 0),
            "a" => ((byte)0, 1),
            "b" => ((byte)0, 2),
            _ => throw new MathExpressionException($"Unknown variable '{name}'.")
        };

        private static float EvalFloat(string formula, FloatResolver resolver)
        {
            var blob = MathExpression.Compile(formula, GenericDataType.Float, ResolveName);
            try
            {
                return blob.Value.Evaluate<float, GenericMathProcessor.FloatProcessor, FloatResolver>(resolver);
            }
            finally
            {
                blob.Dispose();
            }
        }

        // ---- correctness ----

        [Test]
        public void SimpleFormula_WithVariable()
        {
            // x*2+5, x=3 => 11
            Assert.AreEqual(11f, EvalFloat("x*2+5", new FloatResolver { V0 = 3f }), 1e-5f);
        }

        [Test]
        public void Precedence_MultiplyBeforeAdd()
        {
            Assert.AreEqual(14f, EvalFloat("2+3*4", default), 1e-5f);
        }

        [Test]
        public void Parentheses_OverridePrecedence()
        {
            Assert.AreEqual(20f, EvalFloat("(2+3)*4", default), 1e-5f);
        }

        [Test]
        public void Power_IsRightAssociative()
        {
            // 2^3^2 = 2^(3^2) = 2^9 = 512
            Assert.AreEqual(512f, EvalFloat("2^3^2", default), 1e-3f);
        }

        [Test]
        public void UnaryMinus_OnParenGroup()
        {
            Assert.AreEqual(-7f, EvalFloat("-(3+4)", default), 1e-5f);
        }

        [Test]
        public void UnaryMinus_MixedWithMultiply()
        {
            Assert.AreEqual(-6f, EvalFloat("3 * -2", default), 1e-5f);
        }

        [Test]
        public void UnaryMinus_BindsLooserThanPower()
        {
            // -3^2 = -(3^2) = -9 (math convention)
            Assert.AreEqual(-9f, EvalFloat("-3^2", default), 1e-3f);
        }

        [Test]
        public void UnaryMinus_InPowerExponent()
        {
            // 2^-3 = 2^(-3) = 0.125
            Assert.AreEqual(0.125f, EvalFloat("2^-3", default), 1e-4f);
        }

        [Test]
        public void UnaryMinus_BindsTighterThanMultiply()
        {
            // 2 * -3^2 = 2 * -(3^2) = -18
            Assert.AreEqual(-18f, EvalFloat("2*-3^2", default), 1e-3f);
        }

        // ---- functions ----

        [Test]
        public void Function_MaxFloor()
        {
            Assert.AreEqual(3f, EvalFloat("max(floor(7/2),0)", default), 1e-5f);
        }

        [Test]
        public void Function_Log10()
        {
            Assert.AreEqual(3f, EvalFloat("log10(1000)", default), 1e-4f);
        }

        [Test]
        public void Function_Abs()
        {
            Assert.AreEqual(5f, EvalFloat("abs(-5)", default), 1e-5f);
        }

        [Test]
        public void Function_MinWithVariables()
        {
            Assert.AreEqual(2f, EvalFloat("min(a,b)", new FloatResolver { V1 = 2f, V2 = 7f }), 1e-5f);
        }

        // ---- generic type coverage (same formula, different T) ----

        [Test]
        public void Generic_Int()
        {
            var blob = MathExpression.Compile("x*2+5", GenericDataType.Int, ResolveName);
            try
            {
                var result = blob.Value.Evaluate<int, GenericMathProcessor.IntProcessor, IntResolver>(new IntResolver { V0 = 3 });
                Assert.AreEqual(11, result);
            }
            finally
            {
                blob.Dispose();
            }
        }

        [Test]
        public void Generic_BigDouble()
        {
            var blob = MathExpression.Compile("x*2+5", GenericDataType.BigDouble, ResolveName);
            try
            {
                var result = blob.Value.Evaluate<BigDouble, GenericMathProcessor.BigDoubleProcessor, BigDoubleResolver>(
                    new BigDoubleResolver { V0 = new BigDouble(3) });
                Assert.AreEqual(11.0, (double)result, 1e-6);
            }
            finally
            {
                blob.Dispose();
            }
        }

        [Test]
        public void Generic_BigDouble_LnIsNaturalLog()
        {
            // ln must mean the natural log for every T; BigDouble once mapped it to log2 (~1.44x too large)
            var blob = MathExpression.Compile("ln(x)", GenericDataType.BigDouble, ResolveName);
            try
            {
                var result = blob.Value.Evaluate<BigDouble, GenericMathProcessor.BigDoubleProcessor, BigDoubleResolver>(
                    new BigDoubleResolver { V0 = BigDouble.Exp(5) });
                Assert.AreEqual(5.0, (double)result, 1e-6);
            }
            finally
            {
                blob.Dispose();
            }
        }

        [Test]
        public void Generic_BigDouble_PowerKeepsFractionalExponent()
        {
            // ^ must behave like math.pow for every T; BigDouble once truncated the exponent, so x^1.5 returned x
            var blob = MathExpression.Compile("x ^ 1.5", GenericDataType.BigDouble, ResolveName);
            try
            {
                var result = blob.Value.Evaluate<BigDouble, GenericMathProcessor.BigDoubleProcessor, BigDoubleResolver>(
                    new BigDoubleResolver { V0 = new BigDouble(4) });
                Assert.AreEqual(8.0, (double)result, 1e-6);
            }
            finally
            {
                blob.Dispose();
            }
        }

        // ---- constants packing ----

        [Test]
        public void Constants_PackedAsRawDataTypeBytes()
        {
            var blob = MathExpression.Compile("1+2+3", GenericDataType.Float, ResolveName);
            try
            {
                ref var b = ref blob.Value;
                Assert.AreEqual(3 * GenericDataType.Float.GetByteSize(), b.Constants.Length);
                Assert.AreEqual(6f, b.Evaluate<float, GenericMathProcessor.FloatProcessor, FloatResolver>(default), 1e-5f);
            }
            finally
            {
                blob.Dispose();
            }
        }

        // ---- Burst / zero-alloc ----

        [BurstCompile]
        private struct EvalJob : IJob
        {
            public BlobAssetReference<MathExpressionBlob> Blob;
            public FloatResolver Resolver;
            public NativeReference<float> Result;

            public void Execute()
            {
                Result.Value = Blob.Value.Evaluate<float, GenericMathProcessor.FloatProcessor, FloatResolver>(Resolver);
            }
        }

        [Test]
        public void Evaluate_RunsInBurstJob()
        {
            var blob = MathExpression.Compile("(a + b) ^ 2", GenericDataType.Float, ResolveName);
            var result = new NativeReference<float>(Allocator.TempJob);
            try
            {
                new EvalJob
                {
                    Blob = blob,
                    Resolver = new FloatResolver { V1 = 4f, V2 = 1f },
                    Result = result
                }.Run();

                Assert.AreEqual(25f, result.Value, 1e-3f); // (4+1)^2
            }
            finally
            {
                result.Dispose();
                blob.Dispose();
            }
        }

        // ---- malformed input fails loud at compile ----

        [Test]
        public void Malformed_UnbalancedParen_Throws()
        {
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("2+(3*4", GenericDataType.Float, ResolveName));
        }

        [Test]
        public void Malformed_MissingOperand_Throws()
        {
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("2+", GenericDataType.Float, ResolveName));
        }

        [Test]
        public void Malformed_EmptyFormula_Throws()
        {
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("   ", GenericDataType.Float, ResolveName));
        }

        [Test]
        public void Malformed_UnknownVariable_Throws()
        {
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("unknownVar+1", GenericDataType.Float, ResolveName));
        }

        [Test]
        public void Malformed_FunctionMissingParen_Throws()
        {
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("abs 5", GenericDataType.Float, ResolveName));
        }

        [Test]
        public void Malformed_FunctionWrongArity_Throws()
        {
            // min needs 2 args; one arg leaves the operator without operands
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("min(1)", GenericDataType.Float, ResolveName));
            // abs takes 1 arg; two args leave a dangling value on the stack
            Assert.Throws<MathExpressionException>(() => MathExpression.Compile("abs(1,2)", GenericDataType.Float, ResolveName));
        }
    }
}
