namespace AutoSpex.Engine.Operations;

public class GreaterThanOperation : BinaryOperation
{
    public GreaterThanOperation() : base("GreaterThan")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(value) > 0;
    }
}