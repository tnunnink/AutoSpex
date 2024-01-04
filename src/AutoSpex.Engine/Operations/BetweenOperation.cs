namespace AutoSpex.Engine.Operations;

public class BetweenOperation : TernaryOperation
{
    public BetweenOperation() : base("Between")
    {
    }

    protected override bool Evaluate(object? input, object first, object second)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(first) >= 0 && comparable.CompareTo(second) <= 0;
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
}