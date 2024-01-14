namespace AutoSpex.Engine;

public class BetweenOperation() : TernaryOperation("Between")
{
    public override string ShouldMessage => $"Should Be {Name}";
    
    protected override bool Evaluate(object? input, object first, object second)
    {
        if (input is not IComparable comparable) return false;
        return comparable.CompareTo(first) >= 0 && comparable.CompareTo(second) <= 0;
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
}