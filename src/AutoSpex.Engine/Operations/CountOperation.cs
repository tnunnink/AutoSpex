namespace AutoSpex.Engine;

public class CountOperation() : CollectionOperation("Count")
{
    protected override bool Evaluate(IEnumerable<object?> collection, Operation operation,
        params object[] values)
    {
        return operation.Execute(collection.Count(), values);
    }

    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return criterion.Evaluate(collection.Count());
    }
}