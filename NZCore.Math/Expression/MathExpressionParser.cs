// <copyright project="NZCore.Math" file="MathExpressionParser.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NZCore
{
    /// <summary>
    /// Managed, main-thread tokenizer + shunting-yard parser. Turns a formula string into
    /// a flat RPN token list. Not Burst-compatible; runs once at bake time.
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    [Unity.Scripting.LifecycleManagement.NoAutoStaticsCleanup]
#endif
    internal static class MathExpressionParser
    {
        internal enum RpnType : byte
        {
            Number,
            Variable,
            BinaryOp,
            Function,
            Negate
        }

        // internal marker char for unary negation on the operator stack
        private const char UnaryNeg = '~';

        internal struct RpnToken
        {
            public RpnType Type;
            public string Text; // Number literal or Variable name
            public MathOperator Op; // BinaryOp
            public MathFunction Func; // Function
        }

        private enum OpKind : byte
        {
            Operator,
            Function,
            LeftParen
        }

        private struct OpItem
        {
            public OpKind Kind;
            public char Operator; // for Operator
            public string Function; // for Function
        }

        // function names that are reserved and must be followed by '('
        private static readonly Dictionary<string, MathFunction> Functions = new()
        {
            { "abs", MathFunction.Abs },
            { "ceil", MathFunction.Ceil },
            { "floor", MathFunction.Floor },
            { "round", MathFunction.Round },
            { "log10", MathFunction.Log10 },
            { "ln", MathFunction.NaturalLog },
        };

        // 2-arg functions that map to binary MathOperators
        private static readonly Dictionary<string, MathOperator> BinaryFunctions = new()
        {
            { "min", MathOperator.Min },
            { "max", MathOperator.Max },
        };

        public static List<RpnToken> Parse(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new MathExpressionException("Formula is empty.");

            var output = new List<RpnToken>();
            var ops = new Stack<OpItem>();

            // false in operand position (start / after an operator, '(' or ','), true after a
            // value or ')'. Used to disambiguate unary vs binary '-' / '+'.
            var expectOperator = false;

            var i = 0;
            var len = formula.Length;

            while (i < len)
            {
                var c = formula[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (IsNumberStart(c))
                {
                    var number = ScanNumber(formula, ref i);
                    output.Add(new RpnToken { Type = RpnType.Number, Text = number });
                    expectOperator = true;
                    continue;
                }

                if (IsIdentifierStart(c))
                {
                    var name = ScanIdentifier(formula, ref i);

                    if (Functions.ContainsKey(name) || BinaryFunctions.ContainsKey(name))
                    {
                        SkipWhitespace(formula, ref i);
                        if (i >= len || formula[i] != '(')
                            throw new MathExpressionException($"Function '{name}' must be followed by '('.");

                        ops.Push(new OpItem { Kind = OpKind.Function, Function = name });
                        expectOperator = false;
                    }
                    else
                    {
                        output.Add(new RpnToken { Type = RpnType.Variable, Text = name });
                        expectOperator = true;
                    }

                    continue;
                }

                switch (c)
                {
                    case '(':
                        ops.Push(new OpItem { Kind = OpKind.LeftParen });
                        expectOperator = false;
                        i++;
                        break;

                    case ')':
                        PopUntilLeftParen(ops, output);
                        if (ops.Count == 0)
                            throw new MathExpressionException("Mismatched ')'.");
                        ops.Pop(); // discard the left paren
                        if (ops.Count > 0 && ops.Peek().Kind == OpKind.Function)
                            EmitFunction(ops.Pop().Function, output);
                        expectOperator = true;
                        i++;
                        break;

                    case ',':
                        PopUntilLeftParen(ops, output);
                        if (ops.Count == 0)
                            throw new MathExpressionException("Misplaced ',' outside of a function call.");
                        expectOperator = false;
                        i++;
                        break;

                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '^':
                    {
                        if (!expectOperator)
                        {
                            // operand position: only unary +/- are valid here
                            if (c == '-')
                                PushOperator(UnaryNeg, ops, output);
                            else if (c != '+') // unary '+' is a no-op
                                throw new MathExpressionException($"Unexpected operator '{c}'.");

                            i++;
                            // still expecting an operand
                            break;
                        }

                        PushOperator(c, ops, output);
                        expectOperator = false;
                        i++;
                        break;
                    }

                    default:
                        throw new MathExpressionException($"Unexpected character '{c}' at position {i}.");
                }
            }

            while (ops.Count > 0)
            {
                var top = ops.Pop();
                if (top.Kind == OpKind.LeftParen)
                    throw new MathExpressionException("Mismatched '('.");
                if (top.Kind == OpKind.Function)
                    throw new MathExpressionException($"Function '{top.Function}' is missing its ')'.");
                EmitOperator(top.Operator, output);
            }

            return output;
        }

        private static void PushOperator(char op, Stack<OpItem> ops, List<RpnToken> output)
        {
            // a prefix unary operator applies to the operand that follows, so it never
            // pops pending operators on push; only its precedence matters later, when a
            // following binary operator decides whether to pop it.
            if (op == UnaryNeg)
            {
                ops.Push(new OpItem { Kind = OpKind.Operator, Operator = op });
                return;
            }

            var (prec, rightAssoc, _) = OperatorInfo(op);

            while (ops.Count > 0 && ops.Peek().Kind == OpKind.Operator)
            {
                var (topPrec, _, _) = OperatorInfo(ops.Peek().Operator);
                if (topPrec > prec || (topPrec == prec && !rightAssoc))
                    EmitOperator(ops.Pop().Operator, output);
                else
                    break;
            }

            ops.Push(new OpItem { Kind = OpKind.Operator, Operator = op });
        }

        private static void PopUntilLeftParen(Stack<OpItem> ops, List<RpnToken> output)
        {
            while (ops.Count > 0 && ops.Peek().Kind != OpKind.LeftParen)
            {
                var top = ops.Pop();
                if (top.Kind == OpKind.Function)
                    EmitFunction(top.Function, output);
                else
                    EmitOperator(top.Operator, output);
            }
        }

        private static void EmitOperator(char op, List<RpnToken> output)
        {
            if (op == UnaryNeg)
            {
                output.Add(new RpnToken { Type = RpnType.Negate });
                return;
            }

            var (_, _, mathOp) = OperatorInfo(op);
            output.Add(new RpnToken { Type = RpnType.BinaryOp, Op = mathOp });
        }

        private static void EmitFunction(string name, List<RpnToken> output)
        {
            if (BinaryFunctions.TryGetValue(name, out var binOp))
                output.Add(new RpnToken { Type = RpnType.BinaryOp, Op = binOp });
            else if (Functions.TryGetValue(name, out var func))
                output.Add(new RpnToken { Type = RpnType.Function, Func = func });
            else
                throw new MathExpressionException($"Unknown function '{name}'.");
        }

        private static (int prec, bool rightAssoc, MathOperator op) OperatorInfo(char c) =>
            c switch
            {
                '+' => (2, false, MathOperator.Add),
                '-' => (2, false, MathOperator.Subtract),
                '*' => (3, false, MathOperator.Multiply),
                '/' => (3, false, MathOperator.Divide),
                // unary negation: binds tighter than * / but looser than ^,
                // so -3^2 = -(3^2) and 3 * -2 = 3 * (-2). MathOperator field unused.
                UnaryNeg => (4, true, MathOperator.Subtract),
                '^' => (5, true, MathOperator.PowerAtoB),
                _ => throw new MathExpressionException($"Unknown operator '{c}'.")
            };

        private static bool IsNumberStart(char c) => char.IsDigit(c) || c == '.';

        private static bool IsIdentifierStart(char c) => char.IsLetter(c) || c == '_';

        private static string ScanNumber(string s, ref int i)
        {
            var sb = new StringBuilder();
            var seenDot = false;
            var seenExp = false;

            while (i < s.Length)
            {
                var c = s[i];
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                    i++;
                }
                else if (c == '.' && !seenDot && !seenExp)
                {
                    seenDot = true;
                    sb.Append(c);
                    i++;
                }
                else if ((c == 'e' || c == 'E') && !seenExp && sb.Length > 0)
                {
                    seenExp = true;
                    sb.Append(c);
                    i++;
                    if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                    {
                        sb.Append(s[i]);
                        i++;
                    }
                }
                else
                {
                    break;
                }
            }

            var text = sb.ToString();
            if (text.Length == 0 || text == ".")
                throw new MathExpressionException($"Invalid number '{text}'.");

            // validate it parses as a double for early, clear failure
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                throw new MathExpressionException($"Invalid number literal '{text}'.");

            return text;
        }

        private static string ScanIdentifier(string s, ref int i)
        {
            var start = i;
            while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                i++;
            return s.Substring(start, i - start);
        }

        private static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i]))
                i++;
        }
    }

    public class MathExpressionException : Exception
    {
        public MathExpressionException(string message) : base(message) { }
    }
}
