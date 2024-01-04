namespace AutoSpex.Engine.Operations;

public class AllOperation : CollectionOperation
{
    public AllOperation() : base("All")
    {
    }

    protected override bool Evaluate(IEnumerable<object?> collection, Operation operation, params object[] values)
    {
        return collection.Select(item => operation.Execute(item, values)).All(result => result);
    }
    
    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return collection.Select(criterion.Evaluate).Any(result => result);
    }
}