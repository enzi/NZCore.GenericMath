using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace NZCore
{
    public partial struct BigDouble
    {
        private const int NegligibleExponentDiff = 17;

        // 10^-expDiff for aligning the smaller summand, constant data in Burst and independent of PowersOf10.Init
        private static readonly double[] NegativePowersOf10 =
        {
            1e0, 1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6, 1e-7, 1e-8,
            1e-9, 1e-10, 1e-11, 1e-12, 1e-13, 1e-14, 1e-15, 1e-16, 1e-17
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator +(BigDouble a, BigDouble b)
        {
            if (a._mantissa == 0)
            {
                return b;
            }

            if (b._mantissa == 0)
            {
                return a;
            }

            var expDiff = a._exponent - b._exponent;

            if (expDiff > NegligibleExponentDiff)
            {
                return a;
            }

            if (expDiff < -NegligibleExponentDiff)
            {
                return b;
            }

            // Align to the larger exponent
            double mantissa;
            long exponent;

            if (expDiff >= 0)
            {
                exponent = a._exponent;
                mantissa = a._mantissa + b._mantissa * NegativePowersOf10[expDiff];
            }
            else
            {
                exponent = b._exponent;
                mantissa = a._mantissa * NegativePowersOf10[-expDiff] + b._mantissa;
            }

            return NormalizeBelow100(mantissa, exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator -(BigDouble a, BigDouble b) => a + ConstructRaw(-b._mantissa, b._exponent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator *(BigDouble a, BigDouble b) => NormalizeBelow100(a._mantissa * b._mantissa, a._exponent + b._exponent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator /(BigDouble a, BigDouble b)
        {
            var mantissa = a._mantissa / b._mantissa;
            var exponent = a._exponent - b._exponent;
            var abs = math.abs(mantissa);

            // normalized operands divide to |m| in (0.1, 10)
            if (abs >= 1.0 && abs < 10.0)
            {
                return ConstructClamped(mantissa, exponent);
            }

            if (abs >= 0.1 && abs < 1.0)
            {
                return ConstructClamped(mantissa * 10.0, exponent - 1);
            }

            // zero, NaN, division by zero
            return new BigDouble(mantissa, exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator -(BigDouble a) => ConstructRaw(-a._mantissa, a._exponent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator +(BigDouble a) => a;

        // + and * of normalized operands land in |m| < 100, at most one decade above [1, 10).
        // Zero, NaN and cancellation take the general Normalize.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BigDouble NormalizeBelow100(double mantissa, long exponent)
        {
            var abs = math.abs(mantissa);

            if (abs >= 1.0 && abs < 10.0)
            {
                return ConstructClamped(mantissa, exponent);
            }

            if (abs >= 10.0 && abs < 100.0)
            {
                return ConstructClamped(mantissa / 10.0, exponent + 1);
            }

            return new BigDouble(mantissa, exponent);
        }

        // the underflow clamp of Normalize, for mantissas already in [1, 10)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BigDouble ConstructClamped(double mantissa, long exponent) =>
            exponent < long.MinValue / 2 ? Zero : ConstructRaw(mantissa, exponent);
    }
}