using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace NZCore
{
    public partial struct BigDouble
    {
        public static implicit operator BigDouble(byte value) => new BigDouble(value);
        public static implicit operator BigDouble(sbyte value) => new BigDouble(value);
        public static implicit operator BigDouble(short value) => new BigDouble(value);
        public static implicit operator BigDouble(ushort value) => new BigDouble(value);
        public static implicit operator BigDouble(int value) => new BigDouble(value);
        public static implicit operator BigDouble(uint value) => new BigDouble(value);
        public static implicit operator BigDouble(long value) => new BigDouble(value);
        public static implicit operator BigDouble(ulong value) => new BigDouble(value);
        public static implicit operator BigDouble(float value) => new BigDouble(value);
        public static implicit operator BigDouble(double value) => new BigDouble(value);
        public static implicit operator BigDouble(decimal value) => new BigDouble((double)value);
        public static implicit operator BigDouble(half value) => new BigDouble((float)value);
        public static implicit operator half(BigDouble value) => (half)(float)value.ToDouble();
        
        public static explicit operator float(BigDouble value) => (float)value.ToDouble();
        public static explicit operator double(BigDouble value) => value.ToDouble();

        public static explicit operator byte(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= byte.MinValue && d <= byte.MaxValue ? (byte)d : (byte)0;
        }

        public static explicit operator sbyte(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= sbyte.MinValue && d <= sbyte.MaxValue ? (sbyte)d : (sbyte)0;
        }

        public static explicit operator short(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= short.MinValue && d <= short.MaxValue ? (short)d : (short)0;
        }

        public static explicit operator ushort(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= ushort.MinValue && d <= ushort.MaxValue ? (ushort)d : (ushort)0;
        }

        public static explicit operator int(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= int.MinValue && d <= int.MaxValue ? (int)d : 0;
        }

        public static explicit operator uint(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= uint.MinValue && d <= uint.MaxValue ? (uint)d : 0u;
        }

        public static explicit operator long(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= long.MinValue && d <= long.MaxValue ? (long)d : 0L;
        }

        public static explicit operator ulong(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= ulong.MinValue && d <= ulong.MaxValue ? (ulong)d : 0UL;
        }

        
      

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToDouble()
        {
            if (_mantissa == 0)
            {
                return 0.0;
            }

            switch (_exponent)
            {
                case > DoubleExpMax:
                    return _mantissa > 0 ? double.PositiveInfinity : double.NegativeInfinity;
                case < DoubleExpMin:
                    return 0.0;
                case DoubleExpMin:
                    return _mantissa > 0 ? 5e-324 : -5e-324;
            }

            var result = _mantissa * Pow10(_exponent);
            if (!IsFinite(result) || Exponent < 0)
            {
                return result;
            }

            var rounded = math.round(result);
            return math.abs(rounded - result) < 1e-10 ? rounded : result;
        }

        [BurstDiscard]
        public static BigDouble Parse(string value)
        {
            if (value.IndexOf('e') != -1)
            {
                var split = value.Split('e');
                var mantissa = double.Parse(split[0], CultureInfo.InvariantCulture);
                var exponent = long.Parse(split[1], CultureInfo.InvariantCulture);

                return new BigDouble(mantissa, exponent);
            }

            if (value == "NaN")
            {
                return BigDouble.NaN;
            }

            var result = new BigDouble(double.Parse(value, CultureInfo.InvariantCulture));
            if (result.IsNaN)
            {
                throw new Exception("Value is NaN!");
            }

            return result;
        }
    }
}