using System.Collections;

namespace AutoSpex.Engine;

public abstract class TernaryOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, object? value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Ternary operations expect an argument.");

        if (value is not IEnumerable enumerable)
            throw new ArgumentException("Ternary operations expect an enumerable argument.", nameof(value));

        var list = enumerable.Cast<object>().ToList();

        if (list.Count != 2)
            throw new ArgumentException("Ternary operations require exactly two values", nameof(value));

        return Evaluate(input, list[0], list[1]);
    }

    protected abstract bool Evaluate(object? input, object first, object second);
}