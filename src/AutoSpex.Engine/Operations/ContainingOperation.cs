namespace AutoSpex.Engine;

public class ContainingOperation() : BinaryOperation("Containing")
{
    protected override bool Evaluate(object? input, object value)
    {
        if (string.IsNullOrEmpty(value.ToString()))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));

        return input is not null && input.ToString()?.Contains(value.ToString()!) == true;
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}