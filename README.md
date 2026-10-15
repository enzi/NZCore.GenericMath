# com.enzisoft.math

Burst-compatible math for values whose type is only known at runtime, a `BigDouble` type for numbers beyond `double` range, and formulas
that are compiled at bake time and evaluated in jobs.

Data such as stats is often stored as raw bytes plus a `GenericDataType`. This package applies operators, functions and comparisons to those
values directly, so calling code doesn't need its own switch over every numeric type. The per-type code is produced by a source generator
that ships with the package.

All types live in the `NZCore` namespace. Depends on Unity Entities, `com.enzisoft.nzcore` and `com.enzisoft.sourcegenerator.common`.

## Value types

| `GenericDataType`         | C# type                   | Bytes |
|---------------------------|---------------------------|-------|
| `Byte`                    | `byte`                    | 1     |
| `Bool`                    | `bool`                    | 1     |
| `Short`, `UShort`, `Half` | `short`, `ushort`, `half` | 2     |
| `Float`, `Int`, `UInt`    | `float`, `int`, `uint`    | 4     |
| `Double`, `Long`, `ULong` | `double`, `long`, `ulong` | 8     |
| `BigDouble`               | `BigDouble`               | 16    |

`Bool` has no processor. It can be stored and set, but operators don't apply to it.

`GenericUnionValue` holds any of these types in one explicit-layout struct, for passing a value around together with its `GenericDataType`:

```csharp
var a = GenericUnionValue.Create(5f);
var b = GenericUnionValue.Create(GenericDataType.Int, 12.0); // converts the double to int
var c = GenericUnionValue.Create(dataType, valuePtr);         // reads the value at a pointer
```

### The `BIGDOUBLE` define

`BigDouble` only takes part in `GenericDataType` dispatch when the `BIGDOUBLE` scripting define is set (**Project Settings > Player >
Scripting Define Symbols**). The define grows `GenericUnionValue` from 8 to 16 bytes, so only set it if you need `BigDouble` values in
generic data. Without it, `BigDouble` and the typed API still work, but runtime-typed calls don't handle `GenericDataType.BigDouble`.

## Operators, functions and comparisons

- `MathOperator`: `Set`, `Add`, `Subtract`, `Multiply`, `Divide`, `PowerAtoB` (a^b), `PowerBtoA` (b^a), `Min`, `Max`
- `MathFunction`: `Abs`, `Ceil`, `Floor`, `Round`, `Log10`, `NaturalLog`
- `ConditionLogicValueComparison`: `Any` (a != 0), `Equal`, `NotEqual`, `GreaterThan`, `LesserThan`, `GreaterEqual`, `LesserEqual`,
  `HasFlag` ((a & b) == b, on the values cast to `int`)

## Typed API

When the type is known at compile time, pass the value type and its processor. Processors are structs, so Burst resolves and inlines the
call:

```csharp
var sum = MathOperator.Add.Process<float, GenericMathProcessor.FloatProcessor>(a, b);
var rounded = MathFunction.Floor.ProcessMathFunction<float, GenericMathProcessor.FloatProcessor>(x);
bool changed = op.ProcessReturnChange<int, GenericMathProcessor.IntProcessor>(ref value, 5);
```

The generator also adds non-generic overloads per type:

```csharp
var max = GenericMathProcessor.Max(3, 7);
op.Process(ref health, change);
op.ProcessWithMinMax(ref health, change, 0f, maxHealth);
bool changed = op.ProcessReturnChange(ref health, change);
```

Processors exist for `byte`, `short`, `ushort`, `half`, `int`, `uint`, `float`, `long`, `ulong`, `double` and `BigDouble`
(`GenericMathProcessor.<Type>Processor`).

## Runtime-typed API

For values stored as bytes with a `GenericDataType`. When two types differ, the right-hand value is cast to the left-hand value's type.

```csharp
// apply an operator in place
op.Process(dataType, valuePtr, 5f);
op.Process(dataTypeA, ptrA, dataTypeB, ptrB);
op.Process(dataType, valuePtr, GenericUnionValue.Create(dataType, 5.0));

// report whether the stored value changed
bool changed = op.ProcessReturnChange(dataType, valuePtr, 5f);

// clamp the result to [0, *capPtr]; a null capPtr skips the clamp
MathProcessResult result = op.ProcessCapped(dataType, valuePtr, capPtr, 5f);
// result.Changed, result.AtMin (value <= 0), result.AtMax (value >= cap)

// subtract up to amount, return how much was taken
float taken = GenericMath.DrainByRef(dataType, valuePtr, amount, out bool depleted);

bool passed = ConditionLogicValueComparison.GreaterEqual.LogicalComparison(dataType, ptrA, ptrB);
double asDouble = GenericMath.ReadAsDouble(dataType, valuePtr);
union.Cast(GenericDataType.Int, GenericDataType.Float);
```

Each operator also has named shortcuts in `GenericMath`, for example `GenericMath.Add(dataTypeA, ptrA, dataTypeB, ptrB)`,
`GenericMath.AddByRef(dataType, ptr, 5f)` and `GenericMath.AddReturnChange(...)`.

`GenericMath.Set(dataType, ptr, "12.5")` and `GenericDataTypeExtensions.GetByteArray(dataType, "12.5")` parse strings and are managed only,
for authoring and baking.

### Typed loops over runtime-typed values

The runtime-typed calls switch on the data type every time. When many values share one data type, switch once with `GenericMath.Dispatch`
and run a typed loop. The action is a struct, so Burst inlines `Invoke` for each type:

```csharp
unsafe struct AddToAll : IGenericMathAction
{
    public byte* Values;
    public int Count;
    public GenericUnionValue Amount;

    public void Invoke<T, TProcessor>()
        where T : unmanaged, IEquatable<T>
        where TProcessor : struct, IGenericValueCalculator<T>
    {
        var processor = default(TProcessor);
        var amount = UnsafeUtility.As<GenericUnionValue, T>(ref Amount); // every union field starts at offset 0

        for (var i = 0; i < Count; i++)
        {
            ref var value = ref ((T*)Values)[i];
            value = processor.Add(value, amount);
        }
    }
}

var action = new AddToAll { Values = ptr, Count = count, Amount = GenericUnionValue.Create(dataType, 5.0) };
if (!GenericMath.Dispatch(dataType, ref action))
{
    // Bool or None, no processor
}
```

`Dispatch` returns false for `Bool` and `None`. `BigDouble` is only dispatched with the `BIGDOUBLE` define.

Inside an action, `GenericCastUtility.Convert<TFrom, TTo>(value)` converts between two type parameters with the same rules as
`GenericUnionValue.Cast`. It's generated for every type with a processor, Burst reduces each use to its one cast, and other types throw an
`ArgumentException`. `GenericCastUtility.DataTypeOf<T>()` returns the `GenericDataType` of a type parameter, or `None` for types without a
processor such as enums and `bool`. Two nested `Dispatch` calls give an action both value types, for example to apply an operator to values
of different types.

## Atomic operations

`GenericMathAtomic` applies the same operators when several job workers write the same value in one frame. Every call returns the value from
before the change.

```csharp
int before = MathOperator.Add.ProcessAtomic(ref counter, 1);
int beforeClamped = GenericMathAtomic.AddClampedAtomic(ref counter, 1, max);

var old = op.ProcessAtomic(dataType, valuePtr, change);
op.ProcessWithCapacityAtomic(dataType, valuePtr, change, capPtr); // at most *capPtr
op.ProcessWithMinMaxAtomic(dataType, valuePtr, change, capPtr);   // clamped to [0, *capPtr]

var current = GenericMathAtomic.ReadAtomic(dataType, valuePtr);
```

`change` is a `GenericUnionValue` of the same type as the stored value. The operator and the clamp happen in one atomic step. The cap is
read once with a plain read.

- 32 and 64-bit values use a compare-and-swap loop on the value.
- 8 and 16-bit values use a compare-and-swap loop on the 4-byte word that contains them. 16-bit values must be 2-byte aligned.
- `BigDouble` uses an internal striped seqlock. Read concurrently written `BigDouble` values with `ReadAtomic`, since a plain 16-byte read
  can tear.

## Math expressions

`MathExpression` compiles a formula string into a `MathExpressionBlob` at bake time. The blob can then be evaluated in Burst jobs without
allocating.

```csharp
// Bake time (managed, main thread)
var blob = MathExpression.Compile("max(floor(power * 1.5) - armor, 0)", GenericDataType.Float, name => name switch
{
    "power" => ((byte)0, 0),
    "armor" => ((byte)0, 1),
    _ => throw new MathExpressionException($"Unknown variable '{name}'.")
});

// Runtime (Burst)
struct Inputs : IVariableResolver<float>
{
    public float Power, Armor;
    public float Resolve(byte kind, int payload) => payload == 0 ? Power : Armor;
}

float damage = blob.Value.Evaluate<float, GenericMathProcessor.FloatProcessor, Inputs>(new Inputs { Power = 10, Armor = 3 });
```

The name resolver maps each variable to a `(kind, payload)` pair that's stored in the blob. At runtime, your `IVariableResolver<T>` turns
that pair back into a value. What the pair means is up to you, for example a `GenericDataType` and a byte offset into a stat buffer.

To store a formula inside another blob, use `MathExpression.Compile(ref builder, ref root.Formula, formula, dataType, resolveName)`. Blobs
can't hold a `BlobAssetReference`.

Syntax:

- Numbers such as `12`, `1.5`, `.5` and `1e300`. Literals are parsed as the expression's data type, so `1.5` isn't valid in an `Int`
  expression.
- Variables: letters, digits and `_`, not starting with a digit.
- `+`, `-`, `*`, `/`, `^` (power, right-associative), unary `-` and `+`, parentheses.
- Functions: `abs`, `ceil`, `floor`, `round`, `log10`, `ln`, `min(a, b)`, `max(a, b)`.
- Precedence from highest to lowest: `^`, unary `-`, `*` and `/`, `+` and `-`. So `-3^2` is `-9`.

`Compile` throws a `MathExpressionException` for malformed formulas, such as unbalanced parentheses or missing operands.

## BigDouble

A number stored as a `double` mantissa normalized to [1, 10) and a `long` power-of-ten exponent. The range goes far beyond `double`.
Precision is that of a `double`, about 15 to 17 significant digits, so adding two values more than 17 orders of magnitude apart returns the
larger one unchanged.

```csharp
BigDouble gold = 1.5e300;
gold *= BigDouble.Pow(10, 50);  // 1.5e350
gold += 1;                      // no change, 1 is negligible at this scale
FixedString128Bytes text = gold.ToFixedString(); // "1.5e350"
```

- Arithmetic and comparison operators, `IEquatable<BigDouble>` and `IComparable<BigDouble>`.
- Implicit conversion from all numeric types. Explicit conversion to `float`, `double` and the integer types. Integer conversions return 0
  when the value is out of range, and `ToDouble()` returns infinity past `double` range.
- Functions: `Abs`, `Floor`, `Ceil`, `Round`, `Min`, `Max`, `Clamp`, `Sqrt`, `Pow(value, long)`, `Exp`, `Log10`, `Ln`, `Log2`,
  `Log(value, base)` and `Factorial` (Stirling approximation).
- `ToFixedString()` is Burst-compatible. Exponents from -3 to 5 are written as plain decimals, everything else as `1.23e45`.
- `BigDouble.Parse("1.5e300")` is managed only.
- In the Inspector, `BigDouble` fields are a text field that accepts `1.5e300`.

Normalization uses a power-of-ten lookup table. `BigDoubleSystem` creates it when a default or editor world is created and disposes it with
the world. Outside a world, for example in tests, call `BigDouble.PowersOf10.Init()` and `BigDouble.PowersOf10.Dispose()` yourself. Without
the table, lookups fall back to `math.pow`.

## Adding a value type

Processors are partial structs that the generator fills in. To add a type, declare its processor next to the others in
`GenericMathProcessor/DefaultProcessors.cs`:

```csharp
public static partial class GenericMathProcessor
{
    public partial struct FloatProcessor : IGenericValueCalculator<float> { }
}
```

Apart from the arithmetic operators, the generated code calls `Unity.Mathematics.math`. If the type needs different calls, map them with
`[ReplaceMathMethod("old", "new", ...)]`, as `BigDoubleProcessor` does.

For runtime-typed dispatch, the type also needs a `GenericDataType` entry and a `GenericUnionValue` field with the same name plus `Value`
(`Float` → `FloatValue`). The hand-written switches in `GenericMath.cs`, `GenericMathAtomic.cs`, `GenericDataTypeExtensions.cs` and
`LogicalComparisonHelper.cs` need a case as well.

## Tests

`NZCore.Math.Tests` has Edit Mode tests for `BigDouble` and `MathExpression`.
