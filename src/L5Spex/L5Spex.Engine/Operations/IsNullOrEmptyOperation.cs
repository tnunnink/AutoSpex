namespace L5Spex.Engine.Operations;

public class IsNullOrEmptyOperation : UnaryOperation
{
    public IsNullOrEmptyOperation() : base("IsNullOrEmpty")
    {
    }

    protected override bool Evaluate(object? input)
    {
        return string.IsNullOrEmpty(input?.ToString());
    }
}