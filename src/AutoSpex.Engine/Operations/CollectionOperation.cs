using System.Collections;

namespace AutoSpex.Engine;

public abstract class CollectionOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, params object[] values)
    {
        if (input is not IEnumerable enumerable)
            throw new ArgumentException("Collection operations require an enumerable input", nameof(input));

        if (values.Length == 0)
            throw new ArgumentException("Collection operations require at least on inner value", nameof(values));

        return values[0] switch
        {
            Operation operation => Evaluate(enumerable.Cast<object>(), operation, values[1..]),
            Criterion criterion => Evaluate(enumerable.Cast<object>(), criterion),
            _ => Evaluate(enumerable.Cast<object>(), Equal, values)
        };
    }

    protected abstract bool Evaluate(IEnumerable<object?> collection, Operation operation, params object[] values);
    
    protected abstract bool Evaluate(IEnumerable<object?> collection, Criterion criterion);

    protected override bool Supports(TypeGroup group)
    {
        return group == TypeGroup.Collection;
    }

    public override int NumberOfArguments => -1;
}