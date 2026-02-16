using System;
using System.Runtime.CompilerServices;

namespace NZCore
{
    public partial struct BigDouble
    {
        private const int NegligibleExponentDiff = 17;

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

            long expDiff = a._exponent - b._exponent;

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
                mantissa = a._mantissa + b._mantissa * Pow10(-expDiff);
            }
            else
            {
                exponent = b._exponent;
                mantissa = a._mantissa * Pow10(expDiff) + b._mantissa;
            }

            return new BigDouble(mantissa, exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator -(BigDouble a, BigDouble b)
        {
            return a + new BigDouble(-b._mantissa, b._exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator *(BigDouble a, BigDouble b)
        {
            return new BigDouble(a._mantissa * b._mantissa, a._exponent + b._exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator /(BigDouble a, BigDouble b)
        {
            return new BigDouble(a._mantissa / b._mantissa, a._exponent - b._exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator -(BigDouble a)
        {
            return new BigDouble(-a._mantissa, a._exponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigDouble operator +(BigDouble a)
        {
            return a;
        }
    }
}