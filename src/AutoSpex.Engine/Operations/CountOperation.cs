namespace AutoSpex.Engine.Operations;

public class CountOperation : CollectionOperation
{
    public CountOperation() : base("Count")
    {
    }

    protected override bool Evaluate(IEnumerable<object?> collection, Operation operation,
        params object[] values)
    {
        return operation.Execute(collection.Count(), values);
    }
}