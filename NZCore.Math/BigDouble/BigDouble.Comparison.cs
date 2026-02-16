using System;
using System.Runtime.CompilerServices;

namespace NZCore
{
    public partial struct BigDouble
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(BigDouble other)
        {
            // Handle zeros
            if (_mantissa == 0 && other._mantissa == 0) return 0;
            if (_mantissa == 0) return other._mantissa > 0 ? -1 : 1;
            if (other._mantissa == 0) return _mantissa > 0 ? 1 : -1;

            // Different signs
            if (_mantissa > 0 && other._mantissa < 0) return 1;
            if (_mantissa < 0 && other._mantissa > 0) return -1;

            // Same sign — compare exponents first
            int sign = _mantissa > 0 ? 1 : -1;

            if (_exponent > other._exponent) return sign;
            if (_exponent < other._exponent) return -sign;

            // Same exponent — compare mantissas
            if (_mantissa > other._mantissa) return 1;
            if (_mantissa < other._mantissa) return -1;

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BigDouble other)
        {
            return _mantissa == other._mantissa && _exponent == other._exponent;
        }

        public override bool Equals(object obj)
        {
            return obj is BigDouble other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_mantissa.GetHashCode() * 397) ^ _exponent.GetHashCode();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BigDouble a, BigDouble b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BigDouble a, BigDouble b) => !a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(BigDouble a, BigDouble b) => a.CompareTo(b) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(BigDouble a, BigDouble b) => a.CompareTo(b) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(BigDouble a, BigDouble b) => a.CompareTo(b) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(BigDouble a, BigDouble b) => a.CompareTo(b) >= 0;
    }
}