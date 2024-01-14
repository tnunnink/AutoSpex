namespace AutoSpex.Engine;

public class GreaterThanOrEqualOperation() : BinaryOperation("Greater Than Or Equal")
{
    public override string ShouldMessage => $"Should Be {Name}";
    
    protected override bool Evaluate(object? input, object value)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(value) >= 0;
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
}