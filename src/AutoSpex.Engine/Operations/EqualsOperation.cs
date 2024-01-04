namespace AutoSpex.Engine.Operations;

public class EqualsOperation : BinaryOperation
{
    public EqualsOperation() : base("Equal")
    {
    }
    
    protected override bool Evaluate(object? input, object value)
    {
        return input is not null && input.Equals(value);
    }
}