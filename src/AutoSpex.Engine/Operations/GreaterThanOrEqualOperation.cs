namespace AutoSpex.Engine.Operations;

public class GreaterThanOrEqualOperation : BinaryOperation
{
    public GreaterThanOrEqualOperation() : base("Greater Than Or Equal")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(value) >= 0;
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
}