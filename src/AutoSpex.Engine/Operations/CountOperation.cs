namespace AutoSpex.Engine;

public class CountOperation() : CollectionOperation("Count")
{
    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return criterion.Evaluate(collection.Count());
    }
}