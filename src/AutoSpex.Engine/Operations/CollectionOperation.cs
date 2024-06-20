using System.Collections;

namespace AutoSpex.Engine;

public abstract class CollectionOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, params object[] values)
    {
        if (input is not IEnumerable enumerable)
            throw new ArgumentException("Collection operations require an enumerable input", nameof(input));

        if (values.Length != 1)
            throw new ArgumentException("Collection operations require one argument", nameof(values));

        if (values[0] is not Criterion criterion)
            throw new ArgumentException("Collection operations require criterion argument");

        return Evaluate(enumerable.Cast<object?>(), criterion);
    }

    protected abstract bool Evaluate(IEnumerable<object?> collection, Criterion criterion);

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Collection;
}