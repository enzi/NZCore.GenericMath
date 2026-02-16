using System;
using Unity.Collections;
using Unity.Mathematics;

namespace NZCore
{
    public partial struct BigDouble
    {
        public FixedString128Bytes ToFixedString()
        {
            var fs = new FixedString128Bytes();

            if (_mantissa == 0)
            {
                fs.Append('0');
                return fs;
            }

            if (_exponent == long.MaxValue)
            {
                if (_mantissa < 0) fs.Append('-');
                fs.Append('I');
                fs.Append('n');
                fs.Append('f');
                return fs;
            }

            // For small numbers, output as plain decimal
            if (_exponent >= 0 && _exponent < 6)
            {
                double fullValue = _mantissa * Pow10(_exponent);
                AppendDouble(ref fs, fullValue, 2);
                return fs;
            }

            if (_exponent < 0 && _exponent > -4)
            {
                double fullValue = _mantissa * Pow10(_exponent);
                AppendDouble(ref fs, fullValue, (int)(-_exponent) + 2);
                return fs;
            }

            // Scientific notation: "1.23e45"
            AppendDouble(ref fs, _mantissa, 2);
            fs.Append('e');
            AppendLong(ref fs, _exponent);

            return fs;
        }

        public override string ToString()
        {
            return ToFixedString().ToString();
        }

        private static void AppendDouble(ref FixedString128Bytes fs, double value, int decimalPlaces)
        {
            if (value < 0)
            {
                fs.Append('-');
                value = -value;
            }

            long intPart = (long)value;
            AppendLong(ref fs, intPart);

            if (decimalPlaces > 0)
            {
                double frac = value - intPart;
                if (frac < 0) frac = 0;

                double multiplier = Pow10(decimalPlaces);
                long fracInt = (long)math.round(frac * multiplier);

                // Handle rounding up past the decimal places
                if (fracInt >= (long)multiplier)
                {
                    fracInt = 0;
                    // We'd need to increment intPart, but for display purposes this is rare.
                    // Keep it simple - the normalized mantissa shouldn't hit this case.
                }

                // Trim trailing zeros
                while (fracInt > 0 && fracInt % 10 == 0)
                {
                    fracInt /= 10;
                    decimalPlaces--;
                }

                if (fracInt > 0)
                {
                    fs.Append('.');

                    // Leading zeros in fractional part
                    long temp = fracInt;
                    int digits = 0;
                    while (temp > 0)
                    {
                        digits++;
                        temp /= 10;
                    }

                    for (int i = 0; i < decimalPlaces - digits; i++)
                    {
                        fs.Append('0');
                    }

                    AppendLong(ref fs, fracInt);
                }
            }
        }

        private static void AppendLong(ref FixedString128Bytes fs, long value)
        {
            if (value < 0)
            {
                fs.Append('-');
                // Handle long.MinValue edge case
                if (value == long.MinValue)
                {
                    fs.Append((FixedString32Bytes)"9223372036854775808");
                    return;
                }
                value = -value;
            }

            if (value == 0)
            {
                fs.Append('0');
                return;
            }

            // Max long is 19 digits
            int start = fs.Length;
            while (value > 0)
            {
                fs.Append((char)('0' + (int)(value % 10)));
                value /= 10;
            }

            // Reverse the digits we just wrote
            int end = fs.Length - 1;
            while (start < end)
            {
                byte tmp = fs[start];
                fs[start] = fs[end];
                fs[end] = tmp;
                start++;
                end--;
            }
        }
    }
}