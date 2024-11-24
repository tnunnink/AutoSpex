namespace AutoSpex.Engine;

public class LessThanOperation() : CompareOperation("Less Than")
{
    protected override bool Compare(IComparable comparable, object value)
    {
        return comparable.CompareTo(value) < 0;
    }
}