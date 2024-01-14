namespace AutoSpex.Engine;

public class EqualOperation() : BinaryOperation("Equal")
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