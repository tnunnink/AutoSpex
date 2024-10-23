using System.Collections;

namespace AutoSpex.Engine;

public class InOperation() : Operation("In")
{
    public override bool Execute(object? input, object? value)
    {
        if (value is not IEnumerable enumerable)
            throw new ArgumentException("In operation require an enumerable argument value", nameof(value));

        return input is not null && enumerable.Cast<object>().Contains(input);
    }

    protected override bool Supports(TypeGroup group) => group != TypeGroup.Collection && group != TypeGroup.Boolean;
}