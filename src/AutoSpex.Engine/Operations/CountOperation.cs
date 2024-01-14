namespace AutoSpex.Engine;

public class CountOperation() : CollectionOperation("Count")
{
    public override string ShouldMessage => $"Should Have {Name}";

    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return criterion.Evaluate(collection.Count());
    }
}