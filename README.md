# NZCore.GenericMath

NZCore.GenericMath usage is to process 2 numbers with an arbitrary data type.
It can handle most common math operators and functions.

*Some types are defined in NZCore so there's a dependency!*

New types are added like this:

```
public static partial class GenericMathProcessor
{
    public partial struct FloatProcessor : IGenericValueCalculator<float>
    {
    }
}
```

The source generator will generate new methods based on this data type.

The basic usage is calling methods in `GenericMath`.
There are way too many to list but here are the operators and functions:

```
public enum MathOperator : byte
{
    Set,
    Add,
    Subtract,
    Multiply,
    Divide,
    PowerAtoB,
    PowerBtoA,
    Min,
    Max
}

public enum MathFunction : byte
{
    Abs,
    Ceil,
    Floor,
    Round,
    Log10,
    NaturalLog,
}
```
