namespace L5Spex.Engine.Operations;

public class ContainsOperation : BinaryOperation
{
    public ContainsOperation() : base("Contains")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (string.IsNullOrEmpty(value.ToString()))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        
        return input is not null && input.ToString()?.Contains(value.ToString()!) == true;
    }
}