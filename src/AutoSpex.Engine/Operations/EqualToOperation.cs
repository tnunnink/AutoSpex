using L5Sharp.Core;

namespace AutoSpex.Engine;

public class EqualToOperation() : BinaryOperation("Equal To")
{
    protected override bool Evaluate(object? input, object value)
    {
        //If the input is a logix element we will use the EquivalentTo method to compare the entire XMl tree.
        //This is so that it can support entire LogixElement or LogixData structures.
        if (input is LogixElement element)
        {
            var other = value switch
            {
                LogixElement e => e,
                string s => s.TryParse<LogixElement>(),
                _ => null
            };

            return other is not null && element.EquivalentTo(other);
        }

        if (value is string text)
        {
            return Equals(input?.ToString(), text);
        }

        return Equals(input, value);
    }
}