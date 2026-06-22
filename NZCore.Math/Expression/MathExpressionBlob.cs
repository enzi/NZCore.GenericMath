// <copyright project="NZCore.Math" file="MathExpressionBlob.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;

namespace NZCore
{
    public enum InstrType : byte
    {
        // pushes a constant (Payload = byte offset into Constants)
        Const,

        // pushes a variable (Op = user resolver kind, Payload = user offset/id)
        Var,

        // pops 2, pushes 1 (Op = (byte)MathOperator, includes Min/Max)
        BinaryOp,

        // pops 1, pushes 1 (Op = (byte)MathFunction)
        Function,

        // pops 1, pushes 1 (unary negation, computed as 0 - a)
        Negate
    }

    /// <summary>
    /// A single flattened RPN instruction. 8 bytes.
    /// </summary>
    public struct Instr
    {
        public int Payload;
        public InstrType Type;
        public byte Op;
    }

    /// <summary>
    /// Baked, type-agnostic math expression. The instruction stream carries no type
    /// information; evaluation is generic over the value type via
    /// <see cref="MathExpression.Evaluate{T,TProcessor,TResolver}"/>.
    /// Constants are stored as raw <see cref="DataType"/> bytes packed tightly,
    /// mirroring the bytes + datatype storage pattern used elsewhere in the library.
    /// </summary>
    public struct MathExpressionBlob
    {
        public GenericDataType DataType;
        public int MaxStackDepth;
        public BlobArray<Instr> Instructions;
        public BlobArray<byte> Constants;
    }

    /// <summary>
    /// Resolves a variable reference to its value during evaluation. Implement as a
    /// struct and pass its type as the TResolver generic parameter so Burst inlines the
    /// <see cref="Resolve"/> switch. The library never switches on variable source.
    /// </summary>
    public interface IVariableResolver<T>
        where T : unmanaged
    {
        T Resolve(byte kind, int payload);
    }
}
