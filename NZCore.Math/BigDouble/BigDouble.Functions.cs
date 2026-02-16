using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace NZCore
{
    public partial struct BigDouble
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Log10(BigDouble value)
        {
            return value._exponent + math.log10(value._mantissa);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AbsLog10(BigDouble value)
        {
            return value._exponent + math.log10(math.abs(value._mantissa));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Log(BigDouble value, BigDouble @base)
        {
            return Ln(value) / math.log((double)@base);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Log(BigDouble value, double @base)
        {
            return Ln(value) / math.log(@base);
        }

        public static double Log2(BigDouble value)
        {
            return LOG2_10 * Log10(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Ln(BigDouble value)
        {
            if (value._mantissa <= 0)
            {
                return double.NaN;
            }

            return math.log(value._mantissa) + value._exponent * math.LN10_DBL; // ln(10)
        }

        public static BigDouble Pow(BigDouble value, BigDouble power)
        {
            return Pow(value, (long)power);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Pow(BigDouble value, long exp)
        {
            if (value._mantissa == 0)
            {
                return exp == 0 ? One : Zero;
            }

            // 10^(exp * log10(base))
            double log10 = Log10(value);
            double newLog = exp * log10;

            long newExponent = (long)math.floor(newLog);
            double residual = newLog - newExponent;
            double newMantissa = math.pow(10.0, residual);

            return new BigDouble(newMantissa, newExponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Sqrt(BigDouble value)
        {
            if (value._mantissa < 0 || value._mantissa == 0)
            {
                return Zero;
            }

            // If exponent is even, just sqrt the mantissa
            if (value._exponent % 2 == 0)
            {
                return new BigDouble(math.sqrt(value._mantissa), value._exponent / 2);
            }

            // Odd exponent: adjust mantissa by 10 so exponent becomes even
            return new BigDouble(math.sqrt(value._mantissa * 10), (value._exponent - 1) / 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Exp(BigDouble value)
        {
            // e^value = 10^(value * log10(e))
            double log10e = 0.4342944819032518; // log10(e)
            double asDouble = (double)value;

            // For values that fit in double range, use direct computation
            if (value._exponent < 3)
            {
                return new BigDouble(math.exp(asDouble));
            }

            // For large values, use log10 conversion
            double newLog10 = asDouble * log10e;
            long newExponent = (long)math.floor(newLog10);
            double residual = newLog10 - newExponent;
            double newMantissa = math.pow(10.0, residual);

            return new BigDouble(newMantissa, newExponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Abs(BigDouble value)
        {
            return new BigDouble(math.abs(value._mantissa), value._exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Floor(BigDouble value)
        {
            return value._exponent switch
            {
                < 0 => value._mantissa >= 0 ? Zero : NegativeOne,
                >= MaxSignificantExponent => value, // Already integer at this scale
                _ => new BigDouble(math.floor(value.ToDouble()))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Ceil(BigDouble value)
        {
            return value._exponent switch
            {
                < 0 => value._mantissa > 0 ? One : Zero,
                >= MaxSignificantExponent => value, // Already integer at this scale
                _ => new BigDouble(math.ceil(value.ToDouble()))
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Round(BigDouble value)
        {
            if (value._exponent < 0)
            {
                double full = value._mantissa * Pow10(value._exponent);
                return new BigDouble(math.round(full));
            }

            if (value._exponent >= MaxSignificantExponent)
            {
                return value;
            }

            return new BigDouble(math.round(value.ToDouble()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Min(BigDouble a, BigDouble b)
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Max(BigDouble a, BigDouble b)
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble Clamp(BigDouble value, BigDouble min, BigDouble max)
        {
            if (value.CompareTo(min) < 0)
            {
                return min;
            }

            if (value.CompareTo(max) > 0)
            {
                return max;
            }

            return value;
        }

        public static BigDouble Factorial(BigDouble value)
        {
            // https://en.wikipedia.org/wiki/Stirling%27s_approximation#Versions_suitable_for_calculators

            var n = (double)value + 1;
            return Pow(n / math.E_DBL * math.sqrt(n * math.sinh(1 / n) + 1 / (810 * math.pow(n, 6))), n) * math.sqrt(2 * math.PI / n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Pow10(long exponent)
        {
            return PowersOf10.Lookup(exponent);
        }

        public struct PowersOf10
        {
            private static readonly SharedStatic<NativeArray<double>> Powers =
                SharedStatic<NativeArray<double>>.GetOrCreate<PowersOf10, PowersFieldKey>();

            [UsedImplicitly]
            private class PowersFieldKey { }

            private const long IndexOf0 = -DoubleExpMin - 1;

            [BurstDiscard]
            public static void Init()
            {
                if (Powers.Data.IsCreated)
                {
                    return;
                }

                const int count = (int)(DoubleExpMax - DoubleExpMin);
                Powers.Data = new NativeArray<double>(count, Allocator.Persistent);

                for (int i = 0; i < count; i++)
                {
                    Powers.Data[i] = double.Parse("1e" + (i - IndexOf0), CultureInfo.InvariantCulture);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static double Lookup(long power)
            {
                long index = IndexOf0 + power;
                if (index < 0 || index >= Powers.Data.Length)
                {
                    return math.pow(10.0, power);
                }

                return Powers.Data[(int)index];
            }

            public static void Dispose()
            {
                if (Powers.Data.IsCreated)
                {
                    Powers.Data.Dispose();
                }
            }
        }
    }
}