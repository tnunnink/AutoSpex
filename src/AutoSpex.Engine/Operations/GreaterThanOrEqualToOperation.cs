namespace AutoSpex.Engine;

public class GreaterThanOrEqualToOperation() : CompareOperation("Greater Than Or Equal To")
{
    protected override bool Compare(IComparable comparable, object value)
    {
        return comparable.CompareTo(value) >= 0;
    }
}