namespace AutoSpex.Engine.Operations;

public class IsNullOperation : UnaryOperation
{
    public IsNullOperation() : base("IsNull")
    {
    }
    
    protected override bool Evaluate(object? input) => input is null;
}