// <copyright project="NZCore.Math" file="BigDouble.Casts.cs">
// Copyright © 2026 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Mathematics;

namespace NZCore
{
    public partial struct BigDouble
    {
        // Implicit conversions FROM common types TO BigDouble
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

        // Explicit conversions FROM BigDouble TO common types (to prevent data loss)
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

        public static explicit operator float(BigDouble value) => (float)value.ToDouble();

        public static explicit operator double(BigDouble value) => value.ToDouble();

        /*public static explicit operator decimal(BigDouble value)
        {
            var d = value.ToDouble();
            return d >= (double)decimal.MinValue && d <= (double)decimal.MaxValue ? (decimal)d : 0m;
        }*/

        public static implicit operator half(BigDouble value) => (half)(float)value.ToDouble();
    }
}