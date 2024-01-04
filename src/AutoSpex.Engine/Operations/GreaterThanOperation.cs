namespace AutoSpex.Engine.Operations;

public class GreaterThanOperation : BinaryOperation
{
    public GreaterThanOperation() : base("Greater Than")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(value) > 0;
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
}