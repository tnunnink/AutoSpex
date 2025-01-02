using System.Collections;

namespace AutoSpex.Engine;

public abstract class CollectionOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, object? value = default)
    {
        if (input is not IEnumerable enumerable)
            throw new ArgumentException("Collection operations require an enumerable input", nameof(input));

        if (value is not Criterion criterion)
            throw new ArgumentException("Collection operations require a single criterion argument");

        return Evaluate(enumerable.Cast<object>(), criterion);
    }

    protected abstract bool Evaluate(IEnumerable<object> collection, Criterion criterion);

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Collection;
}