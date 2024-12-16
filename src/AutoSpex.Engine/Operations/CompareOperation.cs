namespace AutoSpex.Engine;

public abstract class CompareOperation(string name) : BinaryOperation(name)
{
    protected override bool Evaluate(object? input, object value)
    {
        if (input is null) return false;

        if (input is not IComparable comparable)
            throw new InvalidOperationException($"Input type {input.GetType()} is not a comparable type.");

        return Compare(comparable, value);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Number || group == TypeGroup.Date;
    protected abstract bool Compare(IComparable comparable, object value);
}