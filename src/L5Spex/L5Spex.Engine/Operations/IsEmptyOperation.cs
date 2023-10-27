namespace L5Spex.Engine.Operations;

public class IsEmptyOperation : UnaryOperation
{
    public IsEmptyOperation() : base("IsEmpty")
    {
    }

    protected override bool Evaluate(object? input)
    {
        return input is not null && input switch
        {
            string s => string.Equals(s, string.Empty),
            _ => false
        };
    }
}