using L5Sharp.Core;

namespace AutoSpex.Engine;

public class EquivalentToOperation() : BinaryOperation("Equivalent To")
{
    protected override bool Evaluate(object? input, object value)
    {
        if (input is not LogixElement element) return false;
        
        var other = value switch
        {
            LogixElement e => e,
            string s => s.TryParse<LogixElement>(),
            _ => null
        };

        return other is not null && element.EquivalentTo(other);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Element;
}