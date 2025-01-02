namespace AutoSpex.Engine;

public class GreaterThanOperation() : CompareOperation("Greater Than")
{
    protected override bool Compare(IComparable comparable, object value)
    {
        return comparable.CompareTo(value) > 0;
    }
}