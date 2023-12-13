namespace AutoSpex.Engine.Operations;

public class EqualToOperation : BinaryOperation
{
    public EqualToOperation() : base("EqualTo")
    {
    }
    
    protected override bool Evaluate(object? input, object value)
    {
        return input is not null && input.Equals(value);
    }
}