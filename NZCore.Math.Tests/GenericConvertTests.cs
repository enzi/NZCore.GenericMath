using System;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace NZCore.Math.Tests
{
    [TestFixture]
    public class GenericConvertTests
    {
        private static readonly double[] SampleValues =
        {
            0, 1, -1, 1.5, -2.75, 255, 300, 65535, 70000, -40000, 1e10, -1e10, 3e38, 1e300
        };

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

        [Test]
        public void Convert_FollowsCSharpCasts()
        {
            Assert.AreEqual(2, GenericCastUtility.Convert<float, int>(2.75f));
            Assert.AreEqual((short)-2, GenericCastUtility.Convert<double, short>(-2.75));
            Assert.AreEqual((half)3f, GenericCastUtility.Convert<int, half>(3));
            Assert.AreEqual(float.PositiveInfinity, GenericCastUtility.Convert<double, float>(1e300));
            Assert.AreEqual(42L, GenericCastUtility.Convert<long, long>(42L));
        }

        [Test]
        public void Convert_BigDouble_UsesItsConversionOperators()
        {
            Assert.AreEqual(new BigDouble(1234.0), GenericCastUtility.Convert<int, BigDouble>(1234));
            Assert.AreEqual(1.5e300, GenericCastUtility.Convert<BigDouble, double>(new BigDouble(1.5, 300)));

            // out of range saturates through double, BigDouble's own integer operators would return 0
            Assert.AreEqual(int.MaxValue, GenericCastUtility.Convert<BigDouble, int>(new BigDouble(1, 300)));
        }

        // an out-of-range conversion to an integer type saturates, Burst made it poison and managed code wrapped
        [Test]
        public void Cast_OutOfRangeToInteger_Saturates()
        {
            Assert.AreEqual(255, Cast(GenericUnionValue.Create(1000.3), GenericDataType.Double, GenericDataType.Byte).ByteValue);
            Assert.AreEqual(0, Cast(GenericUnionValue.Create(-3.0), GenericDataType.Double, GenericDataType.Byte).ByteValue);
            Assert.AreEqual(0u, Cast(GenericUnionValue.Create(-3.0), GenericDataType.Double, GenericDataType.UInt).UIntValue);
            Assert.AreEqual(int.MaxValue, Cast(GenericUnionValue.Create(3e9f), GenericDataType.Float, GenericDataType.Int).IntValue);
            Assert.AreEqual(int.MinValue, Cast(GenericUnionValue.Create(-3e9), GenericDataType.Double, GenericDataType.Int).IntValue);
            // the largest double below 2^63, long.MaxValue itself rounds up to 2^63
            Assert.AreEqual(9223372036854774784L, Cast(GenericUnionValue.Create(1e19), GenericDataType.Double, GenericDataType.Long).LongValue);

            Assert.AreEqual(255, Cast(GenericUnionValue.Create(1000), GenericDataType.Int, GenericDataType.Byte).ByteValue);
            Assert.AreEqual(short.MinValue, Cast(GenericUnionValue.Create(-40000), GenericDataType.Int, GenericDataType.Short).ShortValue);
            Assert.AreEqual(0u, Cast(GenericUnionValue.Create(-1), GenericDataType.Int, GenericDataType.UInt).UIntValue);
            Assert.AreEqual(long.MaxValue, Cast(GenericUnionValue.Create(ulong.MaxValue), GenericDataType.ULong, GenericDataType.Long).LongValue);
        }

        // values in range still convert like a C# cast, the fraction is truncated
        [Test]
        public void Cast_InRangeToInteger_Truncates()
        {
            Assert.AreEqual(-2, Cast(GenericUnionValue.Create(-2.75), GenericDataType.Double, GenericDataType.Short).ShortValue);
            Assert.AreEqual(200, Cast(GenericUnionValue.Create(200.9f), GenericDataType.Float, GenericDataType.Byte).ByteValue);
        }

        // the typed Cast and the mixed-type math entry points share the conversion
        [Test]
        public unsafe void TypedCastAndSet_OutOfRangeToByte_Saturate()
        {
            GenericUnionValue value = default;
            value.Cast(1000.3, GenericDataType.Byte);
            Assert.AreEqual(255, value.ByteValue);

            byte stat = 0;
            GenericMath.Set(GenericDataType.Byte, &stat, 300.0);
            Assert.AreEqual(255, stat);
        }

        // in Burst an out-of-range float to integer conversion was poison, a byte read back as 9389
        [Test]
        public void Cast_OutOfRangeToByte_SaturatesInBurst()
        {
            if (!BurstCompiler.IsEnabled)
            {
                Assert.Ignore("Burst compilation is disabled");
            }

            using var result = new NativeArray<int>(2, Allocator.Persistent);

            new SaturatingCastJob { Result = result }.Run();

            Assert.AreEqual(1, result[1], "the job ran managed, so Burst didn't compile Cast");
            Assert.AreEqual(255, result[0]);
        }

        private static GenericUnionValue Cast(GenericUnionValue value, GenericDataType from, GenericDataType to)
        {
            value.Cast(from, to);
            return value;
        }

        [Test]
        public void Convert_UnsupportedType_Throws()
        {
            // a type without a GenericMathProcessor must fail loudly instead of returning reinterpreted bits
            Assert.Throws<ArgumentException>(() => GenericCastUtility.Convert<int, int2>(1));
        }

        // generic code converting inside IGenericMathAction must agree with the runtime-typed GenericUnionValue.Cast
        [Test]
        public void Convert_MatchesRuntimeTypedCast_Managed()
        {
            using var samples = new NativeArray<double>(SampleValues, Allocator.Persistent);
            Assert.AreEqual(0, ConvertComparison.CountMismatches(samples));
        }

        [Test]
        public void Convert_MatchesRuntimeTypedCast_Burst()
        {
            if (!BurstCompiler.IsEnabled)
            {
                Assert.Ignore("Burst compilation is disabled");
            }

            using var samples = new NativeArray<double>(SampleValues, Allocator.Persistent);
            using var result = new NativeArray<int>(2, Allocator.Persistent);

            new ConvertMatchesCastJob
            {
                Samples = samples,
                Result = result
            }.Run();

            Assert.AreEqual(1, result[1], "the job ran managed, so Burst didn't compile Convert");
            Assert.AreEqual(0, result[0]);
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct SaturatingCastJob : IJob
    {
        // [0] the cast byte, [1] 1 when Execute ran Burst-compiled
        public NativeArray<int> Result;

        public void Execute()
        {
            var value = GenericUnionValue.Create(1000.3);
            value.Cast(GenericDataType.Double, GenericDataType.Byte);
            Result[0] = value.ByteValue;

            var ranInBurst = 1;
            MarkManaged(ref ranInBurst);
            Result[1] = ranInBurst;
        }

        [BurstDiscard]
        private static void MarkManaged(ref int ranInBurst)
        {
            ranInBurst = 0;
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct ConvertMatchesCastJob : IJob
    {
        [ReadOnly] public NativeArray<double> Samples;

        // [0] mismatches, [1] 1 when Execute ran Burst-compiled
        public NativeArray<int> Result;

        public void Execute()
        {
            Result[0] = ConvertComparison.CountMismatches(Samples);

            var ranInBurst = 1;
            MarkManaged(ref ranInBurst);
            Result[1] = ranInBurst;
        }

        [BurstDiscard]
        private static void MarkManaged(ref int ranInBurst)
        {
            ranInBurst = 0;
        }
    }

    internal static unsafe class ConvertComparison
    {
        // every pair of types with a processor × every sample, compared byte for byte on the target type's size
        public static int CountMismatches(NativeArray<double> samples)
        {
            var mismatches = 0;

            for (var from = GenericDataType.None; from <= GenericDataType.BigDouble; from++)
            {
                if (!HasProcessor(from))
                {
                    continue;
                }

                for (var to = GenericDataType.None; to <= GenericDataType.BigDouble; to++)
                {
                    if (!HasProcessor(to))
                    {
                        continue;
                    }

                    for (var i = 0; i < samples.Length; i++)
                    {
                        var value = GenericUnionValue.Create(from, samples[i]);

                        var expected = value;
                        expected.Cast(from, to);

                        var action = new ConvertFromAction
                        {
                            To = to,
                            Value = value
                        };

                        GenericMath.Dispatch(from, ref action);

                        if (UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref expected), UnsafeUtility.AddressOf(ref action.Result), action.ResultSize) != 0)
                        {
                            mismatches++;
                        }
                    }
                }
            }

            return mismatches;
        }

        private static bool HasProcessor(GenericDataType dataType)
        {
#if BIGDOUBLE
            return dataType != GenericDataType.None;
#else
            return dataType != GenericDataType.None && dataType != GenericDataType.BigDouble;
#endif
        }

        // outer Dispatch picks TFrom, the inner one TTo, so every pair runs through GenericCastUtility.Convert<TFrom, TTo>
        private struct ConvertFromAction : IGenericMathAction
        {
            public GenericDataType To;
            public GenericUnionValue Value;
            public GenericUnionValue Result;
            public int ResultSize;

            public void Invoke<TFrom, TFromProcessor>()
                where TFrom : unmanaged, IEquatable<TFrom>
                where TFromProcessor : struct, IGenericValueCalculator<TFrom>
            {
                var inner = new ConvertToAction<TFrom>
                {
                    Value = UnsafeUtility.As<GenericUnionValue, TFrom>(ref Value)
                };

                GenericMath.Dispatch(To, ref inner);

                Result = inner.Result;
                ResultSize = inner.ResultSize;
            }
        }

        private struct ConvertToAction<TFrom> : IGenericMathAction
            where TFrom : unmanaged
        {
            public TFrom Value;
            public GenericUnionValue Result;
            public int ResultSize;

            public void Invoke<TTo, TToProcessor>()
                where TTo : unmanaged, IEquatable<TTo>
                where TToProcessor : struct, IGenericValueCalculator<TTo>
            {
                Result = default;
                UnsafeUtility.As<GenericUnionValue, TTo>(ref Result) = GenericCastUtility.Convert<TFrom, TTo>(Value);
                ResultSize = UnsafeUtility.SizeOf<TTo>();
            }
        }
    }
}