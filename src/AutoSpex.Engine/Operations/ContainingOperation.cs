using System.Collections;

namespace AutoSpex.Engine;

public class ContainingOperation() : BinaryOperation("Containing")
{
    protected override bool Evaluate(object? input, object value)
    {
        return input switch
        {
            string text when value is string segment => text.Contains(segment),
            IEnumerable enumerable => enumerable.Cast<object>().Contains(value),
            _ => false
        };
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text || group == TypeGroup.Collection;
}