using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace NZCore
{
    [Serializable]
    public partial struct BigDouble : IEquatable<BigDouble>, IComparable<BigDouble>
    {
        [SerializeField] private double _mantissa;
        [SerializeField] private long _exponent;

        public static readonly BigDouble Zero = new BigDouble(0, 0);
        public static readonly BigDouble One = new BigDouble(1, 0);
        public static readonly BigDouble Ten = new BigDouble(1, 1);
        public static readonly BigDouble NegativeOne = new BigDouble(-1, 0);
        public static readonly BigDouble NaN = new BigDouble(double.NaN, long.MinValue);
        
        public const long DoubleExpMin = -324;
        public const long DoubleExpMax = 308;
        public const double LOG2_10 = 3.32192809488736234787;
        public const int MaxSignificantExponent = 17;

        public double Mantissa => _mantissa;
        public long Exponent => _exponent;
        
        public bool IsZero => _mantissa == 0;
        public bool IsNaN => double.IsNaN(_mantissa);
        
        
        public bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        // used for skipping normalize
        private static BigDouble ConstructRaw(double mantissa, long exponent)
        {
            return new BigDouble
            {
                _mantissa = mantissa,
                _exponent = exponent
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigDouble(BigDouble value)
        {
            _mantissa = value._mantissa;
            _exponent = value._exponent;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigDouble(double mantissa, long exponent)
        {
            _mantissa = mantissa;
            _exponent = exponent;
            Normalize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BigDouble(double value)
        {
            _mantissa = value;
            _exponent = 0;
            Normalize();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            if (double.IsNaN(_mantissa))
            {
                _exponent = 0;
                return;
            }

            if (_mantissa == 0)
            {
                _exponent = 0;
                return;
            }

            if (double.IsInfinity(_mantissa))
            {
                _mantissa = _mantissa > 0 ? 1 : -1;
                _exponent = long.MaxValue;
                return;
            }

            var abs = math.abs(_mantissa);
            if (abs >= 10.0 || abs < 1.0)
            {
                var shift = (long)math.floor(math.log10(abs));

                if (shift == DoubleExpMin)
                {
                    _mantissa = _mantissa * 10.0 / 1e-323;
                }
                else
                {
                    _mantissa /= PowersOf10.Lookup(shift);    
                }
                
                _exponent += shift;

                // Guard against floating-point drift
                // abs = math.abs(_mantissa);
                // if (abs >= 10.0)
                // {
                //     _mantissa /= 10.0;
                //     _exponent++;
                // }
                // else if (abs < 1.0 && _mantissa != 0)
                // {
                //     _mantissa *= 10.0;
                //     _exponent--;
                // }
            }

            if (_exponent < long.MinValue / 2)
            {
                _mantissa = 0;
                _exponent = 0;
            }
        }       
    }
}