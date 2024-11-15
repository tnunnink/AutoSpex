using L5Sharp.Core;

namespace AutoSpex.Engine;

public class EqualToOperation() : BinaryOperation("Equal To")
{
    protected override bool Evaluate(object? input, object value)
    {
        //If the input is a logix element we will use the EquivalentTo method to compare the entire XML tree.
        //This is so that it can support entire LogixElement or LogixData structures.
        //todo I think this actually might need to be split back out but not sure. For now I have to use LogixObject to avoid LogixData elements from using this method
        if (input is LogixObject element)
        {
            var other = value switch
            {
                LogixObject e => e,
                string s => s.TryParse<LogixObject>(),
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