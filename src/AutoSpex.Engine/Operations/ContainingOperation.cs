using System.Collections;

namespace AutoSpex.Engine;

public class ContainingOperation() : BinaryOperation("Containing")
{
    protected override bool Evaluate(object? input, object value)
    {
        if (input is IEnumerable enumerable and not string)
        {
            return enumerable.Cast<object>().Contains(value);
        }

        //We need to call ToString because not all types we consider "text" are literaly string.
        var text = input?.ToString();
        var expected = value.ToString()!;
        return text is not null && text.Contains(expected);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text || group == TypeGroup.Collection;
}