// <copyright project="NZCore.Math" file="GenericMathAtomic.cs">
// Copyright © 2026 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace NZCore
{
    /// <summary>
    /// Atomic twin of <see cref="GenericMath"/> for values that can be written by multiple job workers
    /// in the same frame. Types up to 8 bytes use a lock-free CAS retry loop on the value itself;
    /// BigDouble (16 bytes, no portable 16-byte CAS) uses an internal striped seqlock keyed by the
    /// value address, so call sites never take or even see a lock.
    /// Every operation returns the value that was stored BEFORE it was applied.
    /// </summary>
    public static unsafe class GenericMathAtomic
    {
        #region typed field operations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ProcessAtomic(this MathOperator mathOperator, ref int value, int changeValue)
        {
            if (mathOperator == MathOperator.Set)
            {
                return Interlocked.Exchange(ref value, changeValue);
            }

            while (true)
            {
                var oldValue = Volatile.Read(ref value);
                var newValue = mathOperator.Process<int, GenericMathProcessor.IntProcessor>(oldValue, changeValue);

                if (newValue == oldValue || Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ProcessAtomic(this MathOperator mathOperator, ref uint value, uint changeValue)
        {
            ref var target = ref UnsafeUtility.As<uint, int>(ref value);

            if (mathOperator == MathOperator.Set)
            {
                return (uint)Interlocked.Exchange(ref target, (int)changeValue);
            }

            while (true)
            {
                var oldBits = Volatile.Read(ref target);
                var oldValue = (uint)oldBits;
                var newValue = mathOperator.Process<uint, GenericMathProcessor.UIntProcessor>(oldValue, changeValue);

                if (newValue == oldValue || Interlocked.CompareExchange(ref target, (int)newValue, oldBits) == oldBits)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ProcessAtomic(this MathOperator mathOperator, ref float value, float changeValue)
        {
            ref var target = ref UnsafeUtility.As<float, int>(ref value);

            if (mathOperator == MathOperator.Set)
            {
                return math.asfloat(Interlocked.Exchange(ref target, math.asint(changeValue)));
            }

            while (true)
            {
                var oldBits = Volatile.Read(ref target);
                var oldValue = math.asfloat(oldBits);
                var newValue = mathOperator.Process<float, GenericMathProcessor.FloatProcessor>(oldValue, changeValue);
                var newBits = math.asint(newValue);

                if (newBits == oldBits || Interlocked.CompareExchange(ref target, newBits, oldBits) == oldBits)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ProcessAtomic(this MathOperator mathOperator, ref long value, long changeValue)
        {
            if (mathOperator == MathOperator.Set)
            {
                return Interlocked.Exchange(ref value, changeValue);
            }

            while (true)
            {
                var oldValue = Volatile.Read(ref value);
                var newValue = mathOperator.Process<long, GenericMathProcessor.LongProcessor>(oldValue, changeValue);

                if (newValue == oldValue || Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ProcessAtomic(this MathOperator mathOperator, ref ulong value, ulong changeValue)
        {
            ref var target = ref UnsafeUtility.As<ulong, long>(ref value);

            if (mathOperator == MathOperator.Set)
            {
                return (ulong)Interlocked.Exchange(ref target, (long)changeValue);
            }

            while (true)
            {
                var oldBits = Volatile.Read(ref target);
                var oldValue = (ulong)oldBits;
                var newValue = mathOperator.Process<ulong, GenericMathProcessor.ULongProcessor>(oldValue, changeValue);

                if (newValue == oldValue || Interlocked.CompareExchange(ref target, (long)newValue, oldBits) == oldBits)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ProcessAtomic(this MathOperator mathOperator, ref double value, double changeValue)
        {
            ref var target = ref UnsafeUtility.As<double, long>(ref value);

            if (mathOperator == MathOperator.Set)
            {
                return math.asdouble(Interlocked.Exchange(ref target, math.aslong(changeValue)));
            }

            while (true)
            {
                var oldBits = Volatile.Read(ref target);
                var oldValue = math.asdouble(oldBits);
                var newValue = mathOperator.Process<double, GenericMathProcessor.DoubleProcessor>(oldValue, changeValue);
                var newBits = math.aslong(newValue);

                if (newBits == oldBits || Interlocked.CompareExchange(ref target, newBits, oldBits) == oldBits)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        /// <summary>
        /// Atomically applies value = min(value + amount, max) in a single CAS step,
        /// so the value can never be observed above max. Overflow safe.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddClampedAtomic(ref int value, int amount, int max)
        {
            while (true)
            {
                var oldValue = Volatile.Read(ref value);
                var newValue = (int)math.min((long)oldValue + amount, max);

                if (newValue == oldValue || Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        #endregion

        #region generic pointer operations

        private enum ClampMode : byte
        {
            None,

            // clamp to at most *cappedValuePtr, all operators applied (matches ExecuteNode_ChangeDynamicStat: Process + GenericMath.Min)
            CapOnly,

            // clamp to [0, *cappedValuePtr], Min/Max operators pass the value through (matches GenericMath.ProcessWithMinMax)
            ZeroToCap
        }

        /// <summary>
        /// Atomic equivalent of <see cref="GenericMath.Process(MathOperator,GenericDataType,byte*,GenericUnionValue)"/>.
        /// </summary>
        public static GenericUnionValue ProcessAtomic(this MathOperator mathOperator, GenericDataType dataType, byte* valuePtr, GenericUnionValue changeValue)
        {
            return ProcessAtomicInternal(mathOperator, dataType, valuePtr, changeValue, ClampMode.None, default);
        }

        /// <summary>
        /// Atomic equivalent of Process followed by a min against *cappedValuePtr, applied in ONE atomic step.
        /// The capacity itself is read once up front (plain read, same as the non-atomic sequence).
        /// </summary>
        public static GenericUnionValue ProcessWithCapacityAtomic(this MathOperator mathOperator, GenericDataType dataType, byte* valuePtr, GenericUnionValue changeValue,
            byte* cappedValuePtr)
        {
            var capValue = GenericUnionValue.Create(dataType, cappedValuePtr);
            return ProcessAtomicInternal(mathOperator, dataType, valuePtr, changeValue, ClampMode.CapOnly, capValue);
        }

        /// <summary>
        /// Atomic equivalent of <see cref="GenericMath.ProcessWithMinMax(MathOperator,GenericDataType,byte*,GenericUnionValue,byte*)"/>:
        /// the operator and the clamp to [0, *cappedValuePtr] are applied in ONE atomic step.
        /// The capacity itself is read once up front (plain read, same as the non-atomic version).
        /// </summary>
        public static GenericUnionValue ProcessWithMinMaxAtomic(this MathOperator mathOperator, GenericDataType dataType, byte* valuePtr, GenericUnionValue changeValue,
            byte* cappedValuePtr)
        {
            var capValue = GenericUnionValue.Create(dataType, cappedValuePtr);
            return ProcessAtomicInternal(mathOperator, dataType, valuePtr, changeValue, ClampMode.ZeroToCap, capValue);
        }

        /// <summary>
        /// Consistent read of a value that may be written concurrently by <see cref="ProcessAtomic(MathOperator,GenericDataType,byte*,GenericUnionValue)"/>.
        /// Types up to 8 bytes are naturally torn-free; BigDouble is validated against its seqlock stripe.
        /// </summary>
        public static GenericUnionValue ReadAtomic(GenericDataType dataType, byte* valuePtr)
        {
            switch (dataType)
            {
#if BIGDOUBLE
                case GenericDataType.BigDouble:
                    return new GenericUnionValue { BigDoubleValue = ReadAtomicBigDouble(valuePtr) };
#endif
                default:
                    // aligned reads <= 8 bytes cannot tear
                    return GenericUnionValue.Create(dataType, valuePtr);
            }
        }

        private static GenericUnionValue ProcessAtomicInternal(MathOperator mathOperator, GenericDataType dataType, byte* valuePtr, GenericUnionValue changeValue,
            ClampMode clampMode, GenericUnionValue capValue)
        {
            switch (dataType)
            {
#if BIGDOUBLE
                case GenericDataType.BigDouble:
                    return ProcessAtomicBigDouble(mathOperator, valuePtr, changeValue, clampMode, capValue);
#endif
                case GenericDataType.Byte:
                case GenericDataType.Bool:
                    return ProcessAtomicSubWord(mathOperator, dataType, valuePtr, changeValue, 0xFF, clampMode, capValue);
                case GenericDataType.Short:
                case GenericDataType.UShort:
                case GenericDataType.Half:
                    return ProcessAtomicSubWord(mathOperator, dataType, valuePtr, changeValue, 0xFFFF, clampMode, capValue);
                case GenericDataType.Float:
                case GenericDataType.Int:
                case GenericDataType.UInt:
                    return ProcessAtomic32(mathOperator, dataType, (int*)valuePtr, changeValue, clampMode, capValue);
                case GenericDataType.Double:
                case GenericDataType.Long:
                case GenericDataType.ULong:
                    return ProcessAtomic64(mathOperator, dataType, (long*)valuePtr, changeValue, clampMode, capValue);
                case GenericDataType.None:
                default:
                    throw new ArgumentOutOfRangeException($"For dataType {dataType}");
            }
        }

        private static GenericUnionValue Compute(MathOperator mathOperator, GenericDataType dataType, GenericUnionValue oldValue, GenericUnionValue changeValue,
            ClampMode clampMode, GenericUnionValue capValue)
        {
            switch (clampMode)
            {
                case ClampMode.CapOnly:
                {
                    var newValue = mathOperator.ProcessReturnValue(dataType, oldValue, changeValue);
                    return MathOperator.Min.ProcessReturnValue(dataType, newValue, capValue);
                }
                case ClampMode.ZeroToCap:
                {
                    // mirrors GenericMath.ProcessWithMinMax: Min/Max operators pass the value through, then clamp to [0, cap]
                    var newValue = mathOperator is MathOperator.Min or MathOperator.Max
                        ? oldValue
                        : mathOperator.ProcessReturnValue(dataType, oldValue, changeValue);
                    newValue = MathOperator.Min.ProcessReturnValue(dataType, newValue, capValue);
                    return MathOperator.Max.ProcessReturnValue(dataType, newValue, default);
                }
                case ClampMode.None:
                default:
                    return mathOperator.ProcessReturnValue(dataType, oldValue, changeValue);
            }
        }

        private static GenericUnionValue ProcessAtomic32(MathOperator mathOperator, GenericDataType dataType, int* valuePtr, GenericUnionValue changeValue,
            ClampMode clampMode, GenericUnionValue capValue)
        {
            while (true)
            {
                var oldBits = Volatile.Read(ref *valuePtr);
                var oldValue = new GenericUnionValue { IntValue = oldBits };
                var newValue = Compute(mathOperator, dataType, oldValue, changeValue, clampMode, capValue);
                var newBits = newValue.IntValue;

                if (newBits == oldBits || Interlocked.CompareExchange(ref *valuePtr, newBits, oldBits) == oldBits)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        private static GenericUnionValue ProcessAtomic64(MathOperator mathOperator, GenericDataType dataType, long* valuePtr, GenericUnionValue changeValue,
            ClampMode clampMode, GenericUnionValue capValue)
        {
            while (true)
            {
                var oldBits = Volatile.Read(ref *valuePtr);
                var oldValue = new GenericUnionValue { LongValue = oldBits };
                var newValue = Compute(mathOperator, dataType, oldValue, changeValue, clampMode, capValue);
                var newBits = newValue.LongValue;

                if (newBits == oldBits || Interlocked.CompareExchange(ref *valuePtr, newBits, oldBits) == oldBits)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        // 8/16 bit values cannot be CASed directly, so the CAS runs on the aligned 32 bit word containing them
        private static GenericUnionValue ProcessAtomicSubWord(MathOperator mathOperator, GenericDataType dataType, byte* valuePtr, GenericUnionValue changeValue,
            int valueMask, ClampMode clampMode, GenericUnionValue capValue)
        {
            var address = (ulong)valuePtr;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (valueMask == 0xFFFF && (address & 1) != 0)
            {
                throw new InvalidOperationException("GenericMathAtomic - 16 bit values must be 2-byte aligned");
            }
#endif
            var wordPtr = (int*)(address & ~3ul);
            var bitShift = (int)(address & 3ul) * 8;
            var mask = valueMask << bitShift;

            while (true)
            {
                var oldWord = Volatile.Read(ref *wordPtr);
                var oldValue = new GenericUnionValue { IntValue = (oldWord >> bitShift) & valueMask };
                var newValue = Compute(mathOperator, dataType, oldValue, changeValue, clampMode, capValue);
                var newWord = (oldWord & ~mask) | ((newValue.IntValue & valueMask) << bitShift);

                if (newWord == oldWord || Interlocked.CompareExchange(ref *wordPtr, newWord, oldWord) == oldWord)
                {
                    return oldValue;
                }

                Common.Pause();
            }
        }

        #endregion

#if BIGDOUBLE
        #region BigDouble striped seqlock

        private const int StripeCount = 256; // power of two
        private const int StripeSize = 64; // one cache line per stripe to prevent false sharing

        [StructLayout(LayoutKind.Explicit, Size = StripeCount * StripeSize)]
        private struct SeqLockStripes
        {
        }

        private static readonly SharedStatic<SeqLockStripes> Stripes = SharedStatic<SeqLockStripes>.GetOrCreate<SeqLockStripes>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref int GetStripe(byte* valuePtr)
        {
            var hash = ((ulong)valuePtr >> 4) * 0x9E3779B97F4A7C15ul;
            var index = (int)(hash >> 32) & (StripeCount - 1);
            return ref *(int*)((byte*)Stripes.UnsafeDataPointer + index * StripeSize);
        }

        private static GenericUnionValue ProcessAtomicBigDouble(MathOperator mathOperator, byte* valuePtr, GenericUnionValue changeValue,
            ClampMode clampMode, GenericUnionValue capValue)
        {
            ref var sequence = ref GetStripe(valuePtr);
            var acquired = AcquireSeqLock(ref sequence);

            ref var value = ref *(BigDouble*)valuePtr;
            var oldValue = new GenericUnionValue { BigDoubleValue = value };
            var newValue = Compute(mathOperator, GenericDataType.BigDouble, oldValue, changeValue, clampMode, capValue);
            value = newValue.BigDoubleValue;

            // release: prior data write cannot move below this store
            Volatile.Write(ref sequence, acquired + 1);
            return oldValue;
        }

        public static BigDouble ReadAtomicBigDouble(byte* valuePtr)
        {
            ref var sequence = ref GetStripe(valuePtr);

            while (true)
            {
                var before = Volatile.Read(ref sequence);
                if ((before & 1) == 0) // even = no writer inside
                {
                    var copy = *(BigDouble*)valuePtr; // may tear, validated below
                    Thread.MemoryBarrier();
                    if (Volatile.Read(ref sequence) == before)
                    {
                        return copy;
                    }
                }

                Common.Pause();
            }
        }

        private static int AcquireSeqLock(ref int sequence)
        {
            while (true)
            {
                var current = Volatile.Read(ref sequence);
                // even = free; odd sequence marks a writer inside, readers retry
                if ((current & 1) == 0 && Interlocked.CompareExchange(ref sequence, current + 1, current) == current)
                {
                    return current + 1;
                }

                Common.Pause();
            }
        }

        #endregion
#endif
    }
}
