namespace AutoSpex.Engine.Operations;

public class StartsWithOperation : BinaryOperation
{
    public StartsWithOperation() : base("StartsWith")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (string.IsNullOrEmpty(value.ToString()))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        
        return input is not null && input.ToString()?.StartsWith(value.ToString()!) == true;
    }
}