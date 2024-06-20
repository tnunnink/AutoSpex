namespace AutoSpex.Engine;

public class AllOperation() : CollectionOperation("All")
{
    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return collection.Select(criterion.Evaluate).All(result => result);
    }
}