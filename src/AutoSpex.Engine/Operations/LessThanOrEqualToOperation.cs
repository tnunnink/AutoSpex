namespace AutoSpex.Engine;

public class LessThanOrEqualToOperation() : CompareOperation("Less Than Or Equal To")
{
    protected override bool Compare(IComparable comparable, object value)
    {
        return comparable.CompareTo(value) <= 0;
    }
}