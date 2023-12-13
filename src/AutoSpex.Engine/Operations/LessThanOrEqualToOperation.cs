namespace AutoSpex.Engine.Operations;

public class LessThanOrEqualToOperation : BinaryOperation
{
    public LessThanOrEqualToOperation() : base("LessThanOrEqualTo")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(value) <= 0;
    }
}