using NUnit.Framework;
using NZCore;

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
            BigDouble v = new BigDouble(0.0);
            Assert.AreEqual(0.0, v.Mantissa);
            Assert.AreEqual(0L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_One()
        {
            BigDouble v = new BigDouble(1.0);
            Assert.AreEqual(1.0, v.Mantissa, 1e-15);
            Assert.AreEqual(0L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_LargeNumber()
        {
            BigDouble v = new BigDouble(1234.0);
            Assert.AreEqual(1.234, v.Mantissa, 1e-12);
            Assert.AreEqual(3L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_SmallNumber()
        {
            BigDouble v = new BigDouble(0.005);
            Assert.AreEqual(5.0, v.Mantissa, 1e-12);
            Assert.AreEqual(-3L, v.Exponent);
        }

        [Test]
        public void Constructor_FromDouble_Negative()
        {
            BigDouble v = new BigDouble(-42.0);
            Assert.AreEqual(-4.2, v.Mantissa, 1e-12);
            Assert.AreEqual(1L, v.Exponent);
        }

        [Test]
        public void Constructor_FromParts_Normalizes()
        {
            BigDouble v = new BigDouble(50.0, 3);
            Assert.AreEqual(5.0, v.Mantissa, 1e-12);
            Assert.AreEqual(4L, v.Exponent);
        }

        [Test]
        public void Constructor_FromParts_SmallMantissa()
        {
            BigDouble v = new BigDouble(0.5, 10);
            Assert.AreEqual(5.0, v.Mantissa, 1e-12);
            Assert.AreEqual(9L, v.Exponent);
        }

        [Test]
        public void Constructor_NaN_PreservesNaN()
        {
            BigDouble v = new BigDouble(double.NaN);
            Assert.IsTrue(double.IsNaN((double)v));
        }

        [Test]
        public void Constructor_NaN_FromParts()
        {
            BigDouble v = new BigDouble(double.NaN, 5);
            Assert.IsTrue(double.IsNaN((double)v));
        }

        // ---- Arithmetic ----

        [Test]
        public void Add_SameExponent()
        {
            BigDouble a = new BigDouble(3.0, 5);
            BigDouble b = new BigDouble(4.0, 5);
            BigDouble result = a + b;
            Assert.AreEqual(7.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Add_DifferentExponent()
        {
            BigDouble a = new BigDouble(1.0, 10);
            BigDouble b = new BigDouble(5.0, 8);
            BigDouble result = a + b;
            // 1e10 + 5e8 = 1.05e10
            Assert.AreEqual(1.05, result.Mantissa, 1e-12);
            Assert.AreEqual(10L, result.Exponent);
        }

        [Test]
        public void Add_NegligibleSmaller()
        {
            BigDouble a = new BigDouble(1.0, 100);
            BigDouble b = new BigDouble(1.0, 0);
            BigDouble result = a + b;
            Assert.AreEqual(1.0, result.Mantissa, 1e-12);
            Assert.AreEqual(100L, result.Exponent);
        }

        [Test]
        public void Add_WithZero()
        {
            BigDouble a = new BigDouble(5.0, 3);
            BigDouble result = a + BigDouble.Zero;
            Assert.AreEqual(a.Mantissa, result.Mantissa, 1e-15);
            Assert.AreEqual(a.Exponent, result.Exponent);
        }

        [Test]
        public void Subtract_Basic()
        {
            BigDouble a = new BigDouble(5.0, 5);
            BigDouble b = new BigDouble(3.0, 5);
            BigDouble result = a - b;
            Assert.AreEqual(2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Subtract_ResultNegative()
        {
            BigDouble a = new BigDouble(3.0, 5);
            BigDouble b = new BigDouble(5.0, 5);
            BigDouble result = a - b;
            Assert.AreEqual(-2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Multiply_Basic()
        {
            BigDouble a = new BigDouble(2.0, 5);
            BigDouble b = new BigDouble(3.0, 4);
            BigDouble result = a * b;
            Assert.AreEqual(6.0, result.Mantissa, 1e-12);
            Assert.AreEqual(9L, result.Exponent);
        }

        [Test]
        public void Multiply_LargeExponents()
        {
            BigDouble a = new BigDouble(1.5, 1000);
            BigDouble b = new BigDouble(2.0, 2000);
            BigDouble result = a * b;
            Assert.AreEqual(3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(3000L, result.Exponent);
        }

        [Test]
        public void Multiply_ByZero()
        {
            BigDouble a = new BigDouble(5.0, 100);
            BigDouble result = a * BigDouble.Zero;
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void Divide_Basic()
        {
            BigDouble a = new BigDouble(6.0, 10);
            BigDouble b = new BigDouble(2.0, 3);
            BigDouble result = a / b;
            Assert.AreEqual(3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(7L, result.Exponent);
        }

        [Test]
        public void UnaryMinus()
        {
            BigDouble a = new BigDouble(3.0, 5);
            BigDouble result = -a;
            Assert.AreEqual(-3.0, result.Mantissa, 1e-12);
            Assert.AreEqual(5L, result.Exponent);
        }

        // ---- Comparison ----

        [Test]
        public void Compare_Equal()
        {
            BigDouble a = new BigDouble(5.0, 10);
            BigDouble b = new BigDouble(5.0, 10);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.AreEqual(0, a.CompareTo(b));
        }

        [Test]
        public void Compare_DifferentExponent()
        {
            BigDouble a = new BigDouble(1.0, 10);
            BigDouble b = new BigDouble(9.0, 9);
            Assert.IsTrue(a > b);
            Assert.IsTrue(b < a);
        }

        [Test]
        public void Compare_SameExponent_DifferentMantissa()
        {
            BigDouble a = new BigDouble(3.0, 5);
            BigDouble b = new BigDouble(7.0, 5);
            Assert.IsTrue(a < b);
            Assert.IsTrue(b > a);
        }

        [Test]
        public void Compare_CrossSign()
        {
            BigDouble a = new BigDouble(1.0, 100);
            BigDouble b = new BigDouble(-1.0, 200);
            Assert.IsTrue(a > b);
            Assert.IsTrue(b < a);
        }

        [Test]
        public void Compare_WithZero()
        {
            BigDouble a = new BigDouble(1.0, 5);
            Assert.IsTrue(a > BigDouble.Zero);
            Assert.IsTrue(BigDouble.Zero < a);
        }

        [Test]
        public void Compare_BothNegative()
        {
            BigDouble a = new BigDouble(-1.0, 5);
            BigDouble b = new BigDouble(-1.0, 10);
            Assert.IsTrue(a > b); // -1e5 > -1e10
        }

        [Test]
        public void Compare_LessOrEqual_GreaterOrEqual()
        {
            BigDouble a = new BigDouble(5.0, 10);
            BigDouble b = new BigDouble(5.0, 10);
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
            BigDouble v = new BigDouble(1.5, 3);
            double d = (double)v;
            Assert.AreEqual(1500.0, d, 1e-10);
        }

        [Test]
        public void ExplicitConversion_ToDouble_Large()
        {
            BigDouble v = new BigDouble(1.0, 400);
            double d = (double)v;
            Assert.AreEqual(double.PositiveInfinity, d);
        }

        [Test]
        public void RoundTrip_Double()
        {
            double original = 1234567.89;
            BigDouble v = original;
            double back = (double)v;
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
            BigDouble v = new BigDouble(1.0, 5);
            double log = BigDouble.Log10(v);
            Assert.AreEqual(5.0, log, 1e-12);
        }

        [Test]
        public void Log10_WithMantissa()
        {
            BigDouble v = new BigDouble(5.0, 3);
            double log = BigDouble.Log10(v);
            Assert.AreEqual(System.Math.Log10(5.0) + 3.0, log, 1e-12);
        }

        [Test]
        public void Log10_Negative_ReturnsNaN()
        {
            BigDouble v = new BigDouble(-1.0, 5);
            double log = BigDouble.Log10(v);
            Assert.IsTrue(double.IsNaN(log));
        }

        [Test]
        public void Ln_Basic()
        {
            BigDouble v = new BigDouble(1.0, 0); // value = 1.0
            double ln = BigDouble.Ln(v);
            Assert.AreEqual(0.0, ln, 1e-12);
        }

        [Test]
        public void Pow_Square()
        {
            BigDouble v = new BigDouble(3.0, 2); // 300
            BigDouble result = BigDouble.Pow(v, 2);
            // 300^2 = 90000 = 9.0e4
            Assert.AreEqual(9.0, result.Mantissa, 1e-6);
            Assert.AreEqual(4L, result.Exponent);
        }

        [Test]
        public void Pow_ZeroBase()
        {
            BigDouble result = BigDouble.Pow(BigDouble.Zero, 5);
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void Pow_ZeroExponent()
        {
            BigDouble result = BigDouble.Pow(new BigDouble(5.0, 10), 0);
            Assert.AreEqual(BigDouble.One, result);
        }

        [Test]
        public void Sqrt_PerfectSquare()
        {
            BigDouble v = new BigDouble(9.0, 4); // 9e4 = 90000
            BigDouble result = BigDouble.Sqrt(v);
            // sqrt(90000) = 300 = 3e2
            Assert.AreEqual(3.0, result.Mantissa, 1e-6);
            Assert.AreEqual(2L, result.Exponent);
        }

        [Test]
        public void Sqrt_OddExponent()
        {
            BigDouble v = new BigDouble(1.0, 5); // 1e5
            BigDouble result = BigDouble.Sqrt(v);
            // sqrt(1e5) = ~316.23 = ~3.1623e2
            Assert.AreEqual(2L, result.Exponent);
            Assert.AreEqual(3.16227766, result.Mantissa, 1e-4);
        }

        [Test]
        public void Abs_Positive()
        {
            BigDouble v = new BigDouble(3.0, 5);
            BigDouble result = BigDouble.Abs(v);
            Assert.AreEqual(3.0, result.Mantissa, 1e-15);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Abs_Negative()
        {
            BigDouble v = new BigDouble(-3.0, 5);
            BigDouble result = BigDouble.Abs(v);
            Assert.AreEqual(3.0, result.Mantissa, 1e-15);
            Assert.AreEqual(5L, result.Exponent);
        }

        [Test]
        public void Floor_Basic()
        {
            BigDouble v = new BigDouble(1.5, 0); // 1.5
            BigDouble result = BigDouble.Floor(v);
            Assert.AreEqual(1.0, result.Mantissa, 1e-12);
            Assert.AreEqual(0L, result.Exponent);
        }
        
        [Test]
        public void Floor_High()
        {
            BigDouble v = new BigDouble(1.5, 10000);
            BigDouble result = BigDouble.Floor(v);
            Assert.AreEqual(1.5, result.Mantissa, 1e-12);
            Assert.AreEqual(10000L, result.Exponent);
        }

        [Test]
        public void Ceil_Basic()
        {
            BigDouble v = new BigDouble(1.5, 0); // 1.5
            BigDouble result = BigDouble.Ceil(v);
            Assert.AreEqual(2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(0L, result.Exponent);
        }

        [Test]
        public void Round_Basic()
        {
            BigDouble v = new BigDouble(1.5, 1); // 15
            BigDouble result = BigDouble.Round(v);
            // Math.Round(15) with banker's rounding would be... but 15.0 is exact
            double expected = System.Math.Round(15.0);
            double actual = (double)result;
            Assert.AreEqual(expected, actual, 1e-10);
        }

        [Test]
        public void Min_Max_Clamp()
        {
            BigDouble a = new BigDouble(1.0, 3);
            BigDouble b = new BigDouble(1.0, 5);
            BigDouble c = new BigDouble(1.0, 4);

            Assert.AreEqual(a, BigDouble.Min(a, b));
            Assert.AreEqual(b, BigDouble.Max(a, b));
            Assert.AreEqual(c, BigDouble.Clamp(c, a, b));
            Assert.AreEqual(a, BigDouble.Clamp(a - BigDouble.One, a, b));
        }

        // ---- Edge Cases ----

        [Test]
        public void ZeroTimesAnything()
        {
            BigDouble result = BigDouble.Zero * new BigDouble(1.0, 1000);
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void ZeroPlusAnything()
        {
            BigDouble v = new BigDouble(5.0, 42);
            BigDouble result = BigDouble.Zero + v;
            Assert.AreEqual(v.Mantissa, result.Mantissa, 1e-15);
            Assert.AreEqual(v.Exponent, result.Exponent);
        }

        [Test]
        public void VeryLargeExponent_Multiply()
        {
            BigDouble a = new BigDouble(1.0, 1000000000L);
            BigDouble b = new BigDouble(2.0, 500000000L);
            BigDouble result = a * b;
            Assert.AreEqual(2.0, result.Mantissa, 1e-12);
            Assert.AreEqual(1500000000L, result.Exponent);
        }

        [Test]
        public void TinyPlusHuge()
        {
            BigDouble tiny = new BigDouble(1.0, 0);
            BigDouble huge = new BigDouble(1.0, 50);
            BigDouble result = tiny + huge;
            Assert.AreEqual(1.0, result.Mantissa, 1e-12);
            Assert.AreEqual(50L, result.Exponent);
        }

        [Test]
        public void SubtractEqual_GivesZero()
        {
            BigDouble a = new BigDouble(5.0, 10);
            BigDouble result = a - a;
            Assert.IsTrue(result.IsZero);
        }

        [Test]
        public void NegativeNumber_Multiply()
        {
            BigDouble a = new BigDouble(-3.0, 5);
            BigDouble b = new BigDouble(2.0, 3);
            BigDouble result = a * b;
            Assert.AreEqual(-6.0, result.Mantissa, 1e-12);
            Assert.AreEqual(8L, result.Exponent);
        }

        [Test]
        public void Constructor_DoubleEpsilon()
        {
            // 5e-324 rounds to double.Epsilon (~4.94e-324)
            BigDouble v = new BigDouble(5e-324);
            Assert.IsTrue(v.Mantissa >= 1.0 && v.Mantissa < 10.0, $"Mantissa {v.Mantissa} not normalized");
            Assert.AreEqual(-324L, v.Exponent);
            Assert.IsFalse(v.IsZero);
        }

        [Test]
        public void Constructor_NegativeDoubleEpsilon()
        {
            // -5e-324 rounds to -double.Epsilon (~-4.94e-324)
            BigDouble v = new BigDouble(-5e-324);
            Assert.IsTrue(v.Mantissa <= -1.0 && v.Mantissa > -10.0, $"Mantissa {v.Mantissa} not normalized");
            Assert.AreEqual(-324L, v.Exponent);
            Assert.IsFalse(v.IsZero);
        }

        [Test]
        public void DoubleEpsilon_Arithmetic()
        {
            BigDouble a = new BigDouble(5e-324);
            BigDouble b = new BigDouble(-5e-324);

            // a + b should be zero
            BigDouble sum = a + b;
            Assert.IsTrue(sum.IsZero, $"Expected zero, got {sum.Mantissa}e{sum.Exponent}");

            // a * -1 should equal b
            BigDouble negA = -a;
            Assert.AreEqual(b.Exponent, negA.Exponent);
            Assert.AreEqual(b.Mantissa, negA.Mantissa, 1e-12);
        }

        [Test]
        public void DoubleEpsilon_Comparison()
        {
            BigDouble pos = new BigDouble(5e-324);
            BigDouble neg = new BigDouble(-5e-324);

            Assert.IsTrue(pos > BigDouble.Zero);
            Assert.IsTrue(neg < BigDouble.Zero);
            Assert.IsTrue(pos > neg);
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
            BigDouble v = new BigDouble(1.5, 2); // 150
            string s = v.ToString();
            Assert.AreEqual("150", s);
        }

        [Test]
        public void ToString_LargeExponent()
        {
            BigDouble v = new BigDouble(1.23, 45);
            string s = v.ToString();
            Assert.AreEqual("1.23e45", s);
        }

        [Test]
        public void ToString_NegativeValue()
        {
            BigDouble v = new BigDouble(-2.5, 10);
            string s = v.ToString();
            Assert.AreEqual("-2.5e10", s);
        }

        [Test]
        public void ToString_One()
        {
            Assert.AreEqual("1", BigDouble.One.ToString());
        }
    }
}