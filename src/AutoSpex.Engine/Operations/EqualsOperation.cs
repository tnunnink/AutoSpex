namespace AutoSpex.Engine;

public class EqualsOperation() : BinaryOperation("Equal")
{
    protected override bool Evaluate(object? input, object value)
    {
        return input is not null && input.Equals(value);
    }
}