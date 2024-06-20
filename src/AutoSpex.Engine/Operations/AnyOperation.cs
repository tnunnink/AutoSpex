namespace AutoSpex.Engine;

public class AnyOperation() : CollectionOperation("Any")
{
    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return collection.Select(criterion.Evaluate).Any(result => result);
    }
}