namespace L5Spex.Engine.Operations;

public class GreaterThanOrEqualToOperation : BinaryOperation
{
    public GreaterThanOrEqualToOperation() : base("GreaterThanOrEqualTo")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(value) >= 0;
    }
}