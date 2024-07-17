namespace AutoSpex.Engine;

public class EqualToOperation() : BinaryOperation("Equal To")
{
    protected override bool Evaluate(object? input, object value)
    {
        if (input is null) return false;

        if (value is string s)
        {
            return Equals(input.ToString(), s);
        }

        return Equals(input, value);
    }
}