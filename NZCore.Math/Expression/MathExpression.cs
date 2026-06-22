// <copyright project="NZCore.Math" file="MathExpression.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace NZCore
{
    /// <summary>
    /// Data-driven math expressions: compile a formula string into a type-agnostic
    /// <see cref="MathExpressionBlob"/> at bake time (managed, main thread, once) and
    /// evaluate it at runtime inside Burst jobs with zero allocation.
    /// </summary>
    public static class MathExpression
    {
        /// <summary>
        /// Tokenizes, runs shunting-yard, and flattens a formula into a baked blob.
        /// Runs once on the main thread (not Burst). Constants are parsed into raw
        /// <paramref name="dataType"/> bytes; variables are resolved to a (kind, payload)
        /// pair by <paramref name="resolveName"/> that the runtime resolver interprets.
        /// </summary>
        /// <param name="formula">The expression, e.g. "x * 2 + 5" or "max(floor(a/b), 0)".</param>
        /// <param name="dataType">The value type the expression evaluates as.</param>
        /// <param name="resolveName">Maps a variable name to the kind/payload baked into its VAR instruction.</param>
        public static unsafe BlobAssetReference<MathExpressionBlob> Compile(
            string formula,
            GenericDataType dataType,
            Func<string, (byte kind, int payload)> resolveName)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<MathExpressionBlob>();
            Compile(ref builder, ref root, formula, dataType, resolveName);

            var result = builder.CreateBlobAssetReference<MathExpressionBlob>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

        /// <summary>
        /// Same as <see cref="Compile(string,GenericDataType,Func{string,ValueTuple{byte,int}})"/>
        /// but flattens the expression into a caller-provided <paramref name="builder"/> at the
        /// <paramref name="blob"/> field, so a <see cref="MathExpressionBlob"/> can be nested inside
        /// another blob asset (blobs cannot hold a <see cref="BlobAssetReference{T}"/>).
        /// </summary>
        public static unsafe void Compile(
            ref BlobBuilder builder,
            ref MathExpressionBlob blob,
            string formula,
            GenericDataType dataType,
            Func<string, (byte kind, int payload)> resolveName)
        {
            if (resolveName == null)
                throw new ArgumentNullException(nameof(resolveName));

            var byteSize = dataType.GetByteSize();
            if (byteSize <= 0)
                throw new MathExpressionException($"Unsupported data type '{dataType}'.");

            var rpn = MathExpressionParser.Parse(formula);

            var instructions = new List<Instr>(rpn.Count);
            var constants = new List<byte>();

            var tmp = stackalloc byte[16]; // large enough for any GenericDataType (BigDouble = 16)

            foreach (var token in rpn)
            {
                switch (token.Type)
                {
                    case MathExpressionParser.RpnType.Number:
                    {
                        var offset = constants.Count;
                        GenericMath.Set(dataType, tmp, token.Text);
                        for (var b = 0; b < byteSize; b++)
                            constants.Add(tmp[b]);
                        instructions.Add(new Instr { Type = InstrType.Const, Payload = offset });
                        break;
                    }
                    case MathExpressionParser.RpnType.Variable:
                    {
                        var (kind, payload) = resolveName(token.Text);
                        instructions.Add(new Instr { Type = InstrType.Var, Op = kind, Payload = payload });
                        break;
                    }
                    case MathExpressionParser.RpnType.BinaryOp:
                        instructions.Add(new Instr { Type = InstrType.BinaryOp, Op = (byte)token.Op });
                        break;
                    case MathExpressionParser.RpnType.Function:
                        instructions.Add(new Instr { Type = InstrType.Function, Op = (byte)token.Func });
                        break;
                    case MathExpressionParser.RpnType.Negate:
                        instructions.Add(new Instr { Type = InstrType.Negate });
                        break;
                    default:
                        throw new MathExpressionException($"Unknown token type '{token.Type}'.");
                }
            }

            var maxStackDepth = ComputeAndValidateStackDepth(instructions);

            blob.DataType = dataType;
            blob.MaxStackDepth = maxStackDepth;

            var instrArray = builder.Allocate(ref blob.Instructions, instructions.Count);
            for (var k = 0; k < instructions.Count; k++)
                instrArray[k] = instructions[k];

            var constArray = builder.Allocate(ref blob.Constants, constants.Count);
            for (var k = 0; k < constants.Count; k++)
                constArray[k] = constants[k];
        }

        /// <summary>
        /// Evaluates a baked expression. Burst-compatible and zero-allocation: uses a
        /// stack-allocated evaluation stack and the library's generic processor dispatch.
        /// </summary>
        public static unsafe T Evaluate<T, TProcessor, TResolver>(this ref MathExpressionBlob blob, in TResolver resolver)
            where T : unmanaged, IEquatable<T>
            where TProcessor : struct, IGenericValueCalculator<T>
            where TResolver : struct, IVariableResolver<T>
        {
            var res = resolver; // local copy avoids defensive copies on the 'in' parameter per call
            var stack = stackalloc T[blob.MaxStackDepth];
            var sp = 0;

            var constPtr = (byte*)blob.Constants.GetUnsafePtr();
            ref var instructions = ref blob.Instructions;
            var count = instructions.Length;

            for (var k = 0; k < count; k++)
            {
                ref var instr = ref instructions[k];
                switch (instr.Type)
                {
                    case InstrType.Const:
                        stack[sp++] = *(T*)(constPtr + instr.Payload);
                        break;
                    case InstrType.Var:
                        stack[sp++] = res.Resolve(instr.Op, instr.Payload);
                        break;
                    case InstrType.BinaryOp:
                    {
                        var b = stack[--sp];
                        var a = stack[--sp];
                        stack[sp++] = ((MathOperator)instr.Op).Process<T, TProcessor>(a, b);
                        break;
                    }
                    case InstrType.Function:
                    {
                        var a = stack[--sp];
                        stack[sp++] = ((MathFunction)instr.Op).ProcessMathFunction<T, TProcessor>(a);
                        break;
                    }
                    case InstrType.Negate:
                    {
                        var a = stack[--sp];
                        stack[sp++] = MathOperator.Subtract.Process<T, TProcessor>(default(T), a);
                        break;
                    }
                }
            }

            return stack[0];
        }

        // Simulates the RPN to compute the max evaluation-stack depth and validate operand
        // counts. Mirrors Evaluate exactly: Const/Var push, BinaryOp pops 2 pushes 1,
        // Function pops 1 pushes 1.
        private static int ComputeAndValidateStackDepth(List<Instr> instructions)
        {
            var depth = 0;
            var max = 0;

            foreach (var instr in instructions)
            {
                switch (instr.Type)
                {
                    case InstrType.Const:
                    case InstrType.Var:
                        depth++;
                        break;
                    case InstrType.BinaryOp:
                        depth -= 2;
                        if (depth < 0)
                            throw new MathExpressionException("Malformed expression: operator is missing operands.");
                        depth++;
                        break;
                    case InstrType.Function:
                    case InstrType.Negate:
                        depth -= 1;
                        if (depth < 0)
                            throw new MathExpressionException("Malformed expression: unary operator is missing its argument.");
                        depth++;
                        break;
                }

                if (depth > max)
                    max = depth;
            }

            if (depth != 1)
                throw new MathExpressionException($"Malformed expression: it must reduce to a single value (final stack depth {depth}).");

            return max;
        }
    }
}
