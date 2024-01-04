namespace AutoSpex.Engine.Operations;

public class AnyOperation : CollectionOperation
{
    public AnyOperation() : base("Any")
    {
    }

    protected override bool Evaluate(IEnumerable<object?> collection, Operation operation, params object[] values)
    {
        return collection.Select(item => operation.Execute(item, values)).Any(result => result);
    }

    protected override bool Evaluate(IEnumerable<object?> collection, Criterion criterion)
    {
        return collection.Select(criterion.Evaluate).Any(result => result);
    }
}