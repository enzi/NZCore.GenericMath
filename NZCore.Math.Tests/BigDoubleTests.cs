using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace NZCore.Math.Tests
{
    [TestFixture]
    public class BigDoubleTests
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

        // ---- Construction & Normalization ----

        [Test]
        public void Constructor_FromDouble_Zero()
        {
            var v = new BigDouble(0.0);
            Assert.AreEqual(0.0, v.Mantissa);
            Assert.AreEqual(0L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_One()
        {
            var v = new BigDouble(1.0);
            Assert.AreEqual(1.0, v.Mantissa, 1e-15);
            Assert.AreEqual(0L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_LargeNumber()
        {
            var v = new BigDouble(1234.0);
            Assert.AreEqual(1.234, v.Mantissa, 1e-12);
            Assert.AreEqual(3L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_SmallNumber()
        {
            var v = new BigDouble(0.005);
            Assert.AreEqual(5.0, v.Mantissa, 1e-12);
            Assert.AreEqual(-3L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_Negative()
        {
            var v = new BigDouble(-42.0);
            Assert.AreEqual(-4.2, v.Mantissa, 1e-12);
            Assert.AreEqual(1L, v.Exponent);
        }

        [Test]
        public void Constructor_FromParts_Normalizes()
        {
            var v = new BigDouble(50.0, 3);
            Assert.AreEqual(5.0, v.Mantissa, 1e-12);
            Assert.AreEqual(4L, v.Exponent);
        }

        [Test]
        public void Constructor_FromParts_SmallMantissa()
        {
            var v = new BigDouble(0.5, 10);
            Assert.AreEqual(5.0, v.Mantissa, 1e-12);
            Assert.AreEqual(9L, v.Exponent);
        }

        [Test]
        public void Constructor_NaN_PreservesNaN()
        {
            var v = new BigDouble(double.NaN);
            Assert.IsTrue(double.IsNaN((double)v));
        }

        [Test]
        public void Constructor_NaN_FromParts()
        {
            var v = new BigDouble(double.NaN, 5);
            Assert.IsTrue(double.IsNaN((double)v));
        }

        // ---- Arithmetic ----

        [Test]
        public void Add_SameExponent()
        {
            var a = new BigDouble(3.0, 5);
            var b = new BigDouble(4.0, 5);
            var result = a + b;
            Assert.AreEqual(7.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Add_DifferentExponent()
        {
            var a = new BigDouble(1.0, 10);
            var b = new BigDouble(5.0, 8);
            var result = a + b;
            // 1e10 + 5e8 = 1.05e10
            Assert.AreEqual(1.05, result.Mantissa, 1e-12);
            Assert.AreEqual(10L, result.Exponent);
        }

        [Test]
        public void Add_NegligibleSmaller()
        {
            var a = new BigDouble(1.0, 100);
            var b = new BigDouble(1.0, 0);
            var result = a + b;
            Assert.AreEqual(1.0, result.Mantissa, 1e-12);
            Assert.AreEqual(100L, result.Exponent);
        }

        [Test]
        public void Add_WithZero()
        {
            var a = new BigDouble(5.0, 3);
            var result = a + BigDouble.Zero;
            Assert.AreEqual(a.Mantissa, result.Mantissa, 1e-15);
            Assert.AreEqual(a.Exponent, result.Exponent);
        }

        [Test]
        public void Subtract_Basic()
        {
            var a = new BigDouble(5.0, 5);
            var b = new BigDouble(3.0, 5);
            var result = a - b;
            Assert.AreEqual(2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Subtract_ResultNegative()
        {
            var a = new BigDouble(3.0, 5);
            var b = new BigDouble(5.0, 5);
            var result = a - b;
            Assert.AreEqual(-2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Multiply_Basic()
        {
            var a = new BigDouble(2.0, 5);
            var b = new BigDouble(3.0, 4);
            var result = a * b;
            Assert.AreEqual(6.0, result.Mantissa, 1e-12);
            Assert.AreEqual(9L, result.Exponent);
        }

        [Test]
        public void Multiply_LargeExponents()
        {
            var a = new BigDouble(1.5, 1000);
            var b = new BigDouble(2.0, 2000);
            var result = a * b;
            Assert.AreEqual(3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(3000L, result.Exponent);
        }

        [Test]
        public void Multiply_ByZero()
        {
            var a = new BigDouble(5.0, 100);
            var result = a * BigDouble.Zero;
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void Divide_Basic()
        {
            var a = new BigDouble(6.0, 10);
            var b = new BigDouble(2.0, 3);
            var result = a / b;
            Assert.AreEqual(3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(7L, result.Exponent);
        }

        [Test]
        public void UnaryMinus()
        {
            var a = new BigDouble(3.0, 5);
            var result = -a;
            Assert.AreEqual(-3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        // ---- Comparison ----

        [Test]
        public void Compare_Equal()
        {
            var a = new BigDouble(5.0, 10);
            var b = new BigDouble(5.0, 10);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.AreEqual(0, a.CompareTo(b));
        }

        [Test]
        public void Compare_DifferentExponent()
        {
            var a = new BigDouble(1.0, 10);
            var b = new BigDouble(9.0, 9);
            Assert.IsTrue(a > b);
            Assert.IsTrue(b < a);
        }

        [Test]
        public void Compare_SameExponent_DifferentMantissa()
        {
            var a = new BigDouble(3.0, 5);
            var b = new BigDouble(7.0, 5);
            Assert.IsTrue(a < b);
            Assert.IsTrue(b > a);
        }

        [Test]
        public void Compare_CrossSign()
        {
            var a = new BigDouble(1.0, 100);
            var b = new BigDouble(-1.0, 200);
            Assert.IsTrue(a > b);
            Assert.IsTrue(b < a);
        }

        [Test]
        public void Compare_WithZero()
        {
            var a = new BigDouble(1.0, 5);
            Assert.IsTrue(a > BigDouble.Zero);
            Assert.IsTrue(BigDouble.Zero < a);
        }

        [Test]
        public void Compare_BothNegative()
        {
            var a = new BigDouble(-1.0, 5);
            var b = new BigDouble(-1.0, 10);
            Assert.IsTrue(a > b); // -1e5 > -1e10
        }

        [Test]
        public void Compare_LessOrEqual_GreaterOrEqual()
        {
            var a = new BigDouble(5.0, 10);
            var b = new BigDouble(5.0, 10);
            Assert.IsTrue(a <= b);
            Assert.IsTrue(a >= b);
        }

        // ---- Conversions ----

        [Test]
        public void ImplicitConversion_FromInt()
        {
            BigDouble v = 42;
            Assert.AreEqual(4.2, v.Mantissa, 1e-12);
            Assert.AreEqual(1L, v.Exponent);
        }

        [Test]
        public void ImplicitConversion_FromDouble()
        {
            BigDouble v = 1234.5;
            Assert.AreEqual(1.2345, v.Mantissa, 1e-12);
            Assert.AreEqual(3L, v.Exponent);
        }

        [Test]
        public void ExplicitConversion_ToDouble()
        {
            var v = new BigDouble(1.5, 3);
            var d = (double)v;
            Assert.AreEqual(1500.0, d, 1e-10);
        }

        [Test]
        public void ExplicitConversion_ToDouble_Large()
        {
            var v = new BigDouble(1.0, 400);
            var d = (double)v;
            Assert.AreEqual(double.PositiveInfinity, d);
        }

        [Test]
        public void RoundTrip_Double()
        {
            var original = 1234567.89;
            BigDouble v = original;
            var back = (double)v;
            Assert.AreEqual(original, back, original * 1e-12);
        }

        [Test]
        public void ImplicitConversion_FromLong()
        {
            BigDouble v = 1000000L;
            Assert.AreEqual(1.0, v.Mantissa, 1e-12);
            Assert.AreEqual(6L, v.Exponent);
        }

        // ---- Math Functions ----

        [Test]
        public void Log10_Basic()
        {
            var v = new BigDouble(1.0, 5);
            var log = BigDouble.Log10(v);
            Assert.AreEqual(5.0, log, 1e-12);
        }

        [Test]
        public void Log10_WithMantissa()
        {
            var v = new BigDouble(5.0, 3);
            var log = BigDouble.Log10(v);
            Assert.AreEqual(System.Math.Log10(5.0) + 3.0, log, 1e-12);
        }

        [Test]
        public void Log10_Negative_ReturnsNaN()
        {
            var v = new BigDouble(-1.0, 5);
            var log = BigDouble.Log10(v);
            Assert.IsTrue(double.IsNaN(log));
        }

        [Test]
        public void Ln_Basic()
        {
            var v = new BigDouble(1.0, 0); // value = 1.0
            var ln = BigDouble.Ln(v);
            Assert.AreEqual(0.0, ln, 1e-12);
        }

        [Test]
        public void Pow_Square()
        {
            var v = new BigDouble(3.0, 2); // 300
            var result = BigDouble.Pow(v, 2);
            // 300^2 = 90000 = 9.0e4
            Assert.AreEqual(9.0, result.Mantissa, 1e-6);
            Assert.AreEqual(4L, result.Exponent);
        }

        [Test]
        public void Pow_ZeroBase()
        {
            var result = BigDouble.Pow(BigDouble.Zero, 5);
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void Pow_ZeroExponent()
        {
            var result = BigDouble.Pow(new BigDouble(5.0, 10), 0);
            Assert.AreEqual(BigDouble.One, result);
        }

        [Test]
        public void Pow_FractionalExponent()
        {
            // the exponent was once truncated to long, so 4^0.5 returned 4^0 = 1
            var result = BigDouble.Pow(new BigDouble(4.0), 0.5);
            Assert.AreEqual(2.0, (double)result, 1e-12);
        }

        [Test]
        public void Pow_FractionalExponent_BeyondDoubleRange()
        {
            // 1e300^1.5 = 1e450 overflows double, so the fractional power must stay in log space
            var result = BigDouble.Pow(new BigDouble(1e300), 1.5);
            Assert.AreEqual(450.0, BigDouble.Log10(result), 1e-9);
        }

        [Test]
        public void Pow_BigDoubleExponent_KeepsFraction()
        {
            // the BigDouble-exponent overload cast to long before, dropping the fraction
            var result = BigDouble.Pow(new BigDouble(4.0), new BigDouble(1.5));
            Assert.AreEqual(8.0, (double)result, 1e-12);
        }

        [Test]
        public void Sqrt_PerfectSquare()
        {
            var v = new BigDouble(9.0, 4); // 9e4 = 90000
            var result = BigDouble.Sqrt(v);
            // sqrt(90000) = 300 = 3e2
            Assert.AreEqual(3.0, result.Mantissa, 1e-6);
            Assert.AreEqual(2L, result.Exponent);
        }

        [Test]
        public void Sqrt_OddExponent()
        {
            var v = new BigDouble(1.0, 5); // 1e5
            var result = BigDouble.Sqrt(v);
            // sqrt(1e5) = ~316.23 = ~3.1623e2
            Assert.AreEqual(2L, result.Exponent);
            Assert.AreEqual(3.16227766, result.Mantissa, 1e-4);
        }

        [Test]
        public void Abs_Positive()
        {
            var v = new BigDouble(3.0, 5);
            var result = BigDouble.Abs(v);
            Assert.AreEqual(3.0, result.Mantissa, 1e-15);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Abs_Negative()
        {
            var v = new BigDouble(-3.0, 5);
            var result = BigDouble.Abs(v);
            Assert.AreEqual(3.0, result.Mantissa, 1e-15);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Floor_Basic()
        {
            var v = new BigDouble(1.5, 0); // 1.5
            var result = BigDouble.Floor(v);
            Assert.AreEqual(1.0, result.Mantissa, 1e-12);
            Assert.AreEqual(0L, result.Exponent);
        }

        [Test]
        public void Floor_High()
        {
            var v = new BigDouble(1.5, 10000);
            var result = BigDouble.Floor(v);
            Assert.AreEqual(1.5, result.Mantissa, 1e-12);
            Assert.AreEqual(10000L, result.Exponent);
        }

        [Test]
        public void Ceil_Basic()
        {
            var v = new BigDouble(1.5, 0); // 1.5
            var result = BigDouble.Ceil(v);
            Assert.AreEqual(2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(0L, result.Exponent);
        }

        [Test]
        public void Round_Basic()
        {
            var v = new BigDouble(1.5, 1); // 15
            var result = BigDouble.Round(v);
            // Math.Round(15) with banker's rounding would be... but 15.0 is exact
            var expected = System.Math.Round(15.0);
            var actual = (double)result;
            Assert.AreEqual(expected, actual, 1e-10);
        }

        [Test]
        public void Min_Max_Clamp()
        {
            var a = new BigDouble(1.0, 3);
            var b = new BigDouble(1.0, 5);
            var c = new BigDouble(1.0, 4);

            Assert.AreEqual(a, BigDouble.Min(a, b));
            Assert.AreEqual(b, BigDouble.Max(a, b));
            Assert.AreEqual(c, BigDouble.Clamp(c, a, b));
            Assert.AreEqual(a, BigDouble.Clamp(a - BigDouble.One, a, b));
        }

        // ---- Edge Cases ----

        [Test]
        public void ZeroTimesAnything()
        {
            var result = BigDouble.Zero * new BigDouble(1.0, 1000);
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void ZeroPlusAnything()
        {
            var v = new BigDouble(5.0, 42);
            var result = BigDouble.Zero + v;
            Assert.AreEqual(v.Mantissa, result.Mantissa, 1e-15);
            Assert.AreEqual(v.Exponent, result.Exponent);
        }

        [Test]
        public void VeryLargeExponent_Multiply()
        {
            var a = new BigDouble(1.0, 1000000000L);
            var b = new BigDouble(2.0, 500000000L);
            var result = a * b;
            Assert.AreEqual(2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(1500000000L, result.Exponent);
        }

        [Test]
        public void TinyPlusHuge()
        {
            var tiny = new BigDouble(1.0, 0);
            var huge = new BigDouble(1.0, 50);
            var result = tiny + huge;
            Assert.AreEqual(1.0, result.Mantissa, 1e-12);
            Assert.AreEqual(50L, result.Exponent);
        }

        [Test]
        public void SubtractEqual_GivesZero()
        {
            var a = new BigDouble(5.0, 10);
            var result = a - a;
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void NegativeNumber_Multiply()
        {
            var a = new BigDouble(-3.0, 5);
            var b = new BigDouble(2.0, 3);
            var result = a * b;
            Assert.AreEqual(-6.0, result.Mantissa, 1e-12);
            Assert.AreEqual(8L, result.Exponent);
        }

        [Test]
        public void Constructor_DoubleEpsilon()
        {
            // 5e-324 rounds to double.Epsilon (~4.94e-324)
            var v = new BigDouble(5e-324);
            Assert.IsTrue(v.Mantissa >= 1.0 && v.Mantissa < 10.0, $"Mantissa {v.Mantissa} not normalized");
            Assert.AreEqual(-324L, v.Exponent);
            Assert.IsFalse(v.IsZero);
        }

        [Test]
        public void Constructor_NegativeDoubleEpsilon()
        {
            // -5e-324 rounds to -double.Epsilon (~-4.94e-324)
            var v = new BigDouble(-5e-324);
            Assert.IsTrue(v.Mantissa <= -1.0 && v.Mantissa > -10.0, $"Mantissa {v.Mantissa} not normalized");
            Assert.AreEqual(-324L, v.Exponent);
            Assert.IsFalse(v.IsZero);
        }

        [Test]
        public void DoubleEpsilon_Arithmetic()
        {
            var a = new BigDouble(5e-324);
            var b = new BigDouble(-5e-324);

            // a + b should be zero
            var sum = a + b;
            Assert.IsTrue(sum.IsZero, $"Expected zero, got {sum.Mantissa}e{sum.Exponent}");

            // a * -1 should equal b
            var negA = -a;
            Assert.AreEqual(b.Exponent, negA.Exponent);
            Assert.AreEqual(b.Mantissa, negA.Mantissa, 1e-12);
        }

        [Test]
        public void DoubleEpsilon_Comparison()
        {
            var pos = new BigDouble(5e-324);
            var neg = new BigDouble(-5e-324);

            Assert.IsTrue(pos > BigDouble.Zero);
            Assert.IsTrue(neg < BigDouble.Zero);
            Assert.IsTrue(pos > neg);
        }

        // ---- Operator Normalization ----
        // Equals compares the raw fields and CompareTo compares exponents first,
        // so every result has to be canonical: (0, 0), NaN or |mantissa| in [1, 10).

        private const int OperatorCount = 5;

        [Test]
        public void Operators_ResultsAreCanonical([Values] bool burst)
        {
            using var operands = CreateOperands();
            using var results = new NativeArray<BigDouble>(operands.Length * operands.Length * OperatorCount, Allocator.Persistent);
            using var references = new NativeArray<BigDouble>(results.Length, Allocator.Persistent);
            RunOperators(burst, operands, results, references);

            var failures = new List<string>();
            for (var i = 0; i < results.Length; i++)
            {
                var r = results[i];
                var abs = System.Math.Abs(r.Mantissa);
                var canonical = r.IsNaN || (r.Mantissa == 0 && r.Exponent == 0) || (abs >= 1.0 && abs < 10.0);
                if (!canonical)
                {
                    failures.Add(Describe(operands, i, r));
                }
            }

            Assert.IsEmpty(failures, $"{failures.Count} non-canonical results");
        }

        [Test]
        public void Operators_MatchGeneralNormalizeWithinOneUlp([Values] bool burst)
        {
            using var operands = CreateOperands();
            using var results = new NativeArray<BigDouble>(operands.Length * operands.Length * OperatorCount, Allocator.Persistent);
            using var references = new NativeArray<BigDouble>(results.Length, Allocator.Persistent);
            RunOperators(burst, operands, results, references);

            var failures = new List<string>();
            for (var i = 0; i < results.Length; i++)
            {
                var r = results[i];
                var expected = references[i];

                bool match;
                if (r.IsNaN || expected.IsNaN)
                {
                    match = r.IsNaN && expected.IsNaN;
                }
                else if (r.Mantissa == 0 || expected.Mantissa == 0)
                {
                    match = r.Mantissa == 0 && expected.Mantissa == 0 && r.Exponent == expected.Exponent;
                }
                else
                {
                    var ulps = BitConverter.DoubleToInt64Bits(r.Mantissa) - BitConverter.DoubleToInt64Bits(expected.Mantissa);
                    match = r.Exponent == expected.Exponent && ulps >= -1 && ulps <= 1;
                }

                if (!match)
                {
                    failures.Add($"{Describe(operands, i, r)}, general path {expected.Mantissa:R}e{expected.Exponent}");
                }
            }

            Assert.IsEmpty(failures, $"{failures.Count} results differ from the general Normalize path");
        }

        [Test]
        public void PowersOfTen_CompareEqualToMantissaOne([Values] bool burst)
        {
            // normal doubles only, a subnormal 1eK isn't close enough to 10^K to be expected as (1, K)
            const int minPower = -307;
            const int maxPower = 308;

            var powers = new double[maxPower - minPower + 1];
            for (var i = 0; i < powers.Length; i++)
            {
                powers[i] = double.Parse("1e" + (minPower + i), CultureInfo.InvariantCulture);
            }

            using var values = new NativeArray<double>(powers, Allocator.Persistent);
            using var results = new NativeArray<BigDouble>(values.Length, Allocator.Persistent);

            var job = new FromDoubleJob { Values = values, Results = results };
            if (burst)
            {
                Assume.That(BurstCompiler.IsEnabled, "Burst compilation is disabled");
                job.Run();
            }
            else
            {
                job.Execute();
            }

            var failures = new List<string>();
            for (var i = 0; i < values.Length; i++)
            {
                var power = minPower + i;
                var expected = new BigDouble(1.0, power);
                if (results[i] != expected || results[i].CompareTo(expected) != 0)
                {
                    failures.Add($"1e{power} -> {results[i].Mantissa:R}e{results[i].Exponent}");
                }
            }

            Assert.IsEmpty(failures, $"{failures.Count} powers of ten differ from (1, k)");
        }

        [Test]
        public void Add_DoesNotNeedPowersOf10Init()
        {
            // same sign, so no sum cancels and needs the general Normalize path
            var a = new BigDouble(3.0, 20);
            var expected = new BigDouble[18];
            var actual = new BigDouble[18];

            for (var diff = 0; diff < expected.Length; diff++)
            {
                expected[diff] = a + new BigDouble(7.0, 20 - diff);
            }

            BigDouble.PowersOf10.Dispose();
            try
            {
                for (var diff = 0; diff < actual.Length; diff++)
                {
                    actual[diff] = a + new BigDouble(7.0, 20 - diff);
                }
            }
            finally
            {
                BigDouble.PowersOf10.Init();
            }

            CollectionAssert.AreEqual(expected, actual);
        }

        private static NativeArray<BigDouble> CreateOperands()
        {
            var mantissas = new[] { 1.0, 1.0000000000000002, 1.5, 3.3333333333333335, 5.0, 9.999999999999998 };

            // long.MinValue / 4 and one below it put products and quotients right at the underflow clamp
            var exponents = new[] { long.MinValue / 4 - 1, long.MinValue / 4, -300L, -17L, -1L, 0L, 1L, 16L, 17L, 18L, long.MaxValue / 4 };

            var operands = new NativeArray<BigDouble>(mantissas.Length * exponents.Length * 2 + 2, Allocator.Persistent);
            var index = 0;
            foreach (var mantissa in mantissas)
            {
                foreach (var exponent in exponents)
                {
                    operands[index++] = new BigDouble(mantissa, exponent);
                    operands[index++] = new BigDouble(-mantissa, exponent);
                }
            }

            operands[index++] = BigDouble.Zero;
            operands[index] = BigDouble.NaN;
            return operands;
        }

        private static void RunOperators(bool burst, NativeArray<BigDouble> operands, NativeArray<BigDouble> results, NativeArray<BigDouble> references)
        {
            var job = new OperatorJob { Operands = operands, Results = results, References = references };
            if (burst)
            {
                Assume.That(BurstCompiler.IsEnabled, "Burst compilation is disabled");
                job.Run();
            }
            else
            {
                job.Execute();
            }
        }

        private static string Describe(NativeArray<BigDouble> operands, int resultIndex, BigDouble result)
        {
            var pair = resultIndex / OperatorCount;
            var a = operands[pair / operands.Length];
            var b = operands[pair % operands.Length];
            var op = (resultIndex % OperatorCount) switch
            {
                0 => "+",
                1 => "-",
                2 => "*",
                3 => "/",
                _ => "neg"
            };

            return $"({a.Mantissa:R}e{a.Exponent}) {op} ({b.Mantissa:R}e{b.Exponent}) = {result.Mantissa:R}e{result.Exponent}";
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct OperatorJob : IJob
        {
            [ReadOnly] public NativeArray<BigDouble> Operands;
            public NativeArray<BigDouble> Results;
            public NativeArray<BigDouble> References;

            public void Execute()
            {
                for (var i = 0; i < Operands.Length; i++)
                {
                    for (var j = 0; j < Operands.Length; j++)
                    {
                        var a = Operands[i];
                        var b = Operands[j];
                        var index = (i * Operands.Length + j) * OperatorCount;

                        Results[index] = a + b;
                        Results[index + 1] = a - b;
                        Results[index + 2] = a * b;
                        Results[index + 3] = a / b;
                        Results[index + 4] = -a;

                        References[index] = ReferenceAdd(a, b);
                        References[index + 1] = ReferenceAdd(a, new BigDouble(-b.Mantissa, b.Exponent));
                        References[index + 2] = new BigDouble(a.Mantissa * b.Mantissa, a.Exponent + b.Exponent);
                        References[index + 3] = new BigDouble(a.Mantissa / b.Mantissa, a.Exponent - b.Exponent);
                        References[index + 4] = new BigDouble(-a.Mantissa, a.Exponent);
                    }
                }
            }

            // operator + before the fast paths, every sum goes through the general Normalize path
            private static BigDouble ReferenceAdd(BigDouble a, BigDouble b)
            {
                if (a.Mantissa == 0)
                {
                    return b;
                }

                if (b.Mantissa == 0)
                {
                    return a;
                }

                var expDiff = a.Exponent - b.Exponent;
                if (expDiff > 17)
                {
                    return a;
                }

                if (expDiff < -17)
                {
                    return b;
                }

                return expDiff >= 0
                    ? new BigDouble(a.Mantissa + b.Mantissa * BigDouble.PowersOf10.Lookup(-expDiff), a.Exponent)
                    : new BigDouble(a.Mantissa * BigDouble.PowersOf10.Lookup(expDiff) + b.Mantissa, b.Exponent);
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct FromDoubleJob : IJob
        {
            [ReadOnly] public NativeArray<double> Values;
            public NativeArray<BigDouble> Results;

            public void Execute()
            {
                for (var i = 0; i < Values.Length; i++)
                {
                    Results[i] = new BigDouble(Values[i]);
                }
            }
        }

        // ---- ToString ----

        [Test]
        public void ToString_Zero()
        {
            Assert.AreEqual("0", BigDouble.Zero.ToString());
        }

        [Test]
        public void ToString_SmallNumber()
        {
            var v = new BigDouble(1.5, 2); // 150
            var s = v.ToString();
            Assert.AreEqual("150", s);
        }

        [Test]
        public void ToString_LargeExponent()
        {
            var v = new BigDouble(1.23, 45);
            var s = v.ToString();
            Assert.AreEqual("1.23e45", s);
        }

        [Test]
        public void ToString_NegativeValue()
        {
            var v = new BigDouble(-2.5, 10);
            var s = v.ToString();
            Assert.AreEqual("-2.5e10", s);
        }

        [Test]
        public void ToString_One()
        {
            Assert.AreEqual("1", BigDouble.One.ToString());
        }
    }
}