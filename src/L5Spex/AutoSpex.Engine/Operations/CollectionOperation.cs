using System.Collections;

namespace AutoSpex.Engine.Operations;

public abstract class CollectionOperation : Operation
{
    protected CollectionOperation(string name) : base(name)
    {
    }

    public override bool Execute(object? input, params object[] values)
    {
        if (input is not IEnumerable enumerable)
            throw new ArgumentException("Collection operations require an enumerable input", nameof(input));

        if (values.Length > 0 && values[0] is Operation operation)
        {
            return Evaluate(enumerable.Cast<object>(), operation, values[1..]);
        }

        return Evaluate(enumerable.Cast<object>(), EqualTo, values);
    }

    protected abstract bool Evaluate(IEnumerable<object?> collection, Operation operation, params object[] values);

    public override bool Supports(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type);
    }
}