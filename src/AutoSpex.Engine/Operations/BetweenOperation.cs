namespace AutoSpex.Engine;

public class BetweenOperation() : BinaryOperation("Between")
{
    protected override bool Evaluate(object? input, object value)
    {
        if (value is not Range range)
            throw new ArgumentException($"Between operation expects a {typeof(Range)} argument.");

        return range.InRange(input);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
}