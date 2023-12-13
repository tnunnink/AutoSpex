namespace AutoSpex.Engine.Operations;

public class IsNullOrWhiteSpaceOperation : UnaryOperation
{
    public IsNullOrWhiteSpaceOperation() : base("IsNullOrWhiteSpace")
    {
    }

    protected override bool Evaluate(object? input)
    {
        return string.IsNullOrWhiteSpace(input?.ToString());
    }
}