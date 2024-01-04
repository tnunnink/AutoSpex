using System.Text.RegularExpressions;

namespace AutoSpex.Engine.Operations;

public class IsMatchOperation : BinaryOperation
{
    public IsMatchOperation() : base("Is Match")
    {
    }

    protected override bool Evaluate(object? input, object value)
    {
        if (value.ToString() is null) 
            throw new ArgumentNullException(nameof(value));
        
        return input?.ToString() is not null && Regex.IsMatch(input.ToString()!, value.ToString()!);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}