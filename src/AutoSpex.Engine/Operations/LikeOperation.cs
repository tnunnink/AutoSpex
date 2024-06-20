using System.Text.RegularExpressions;

namespace AutoSpex.Engine;

public class LikeOperation() : BinaryOperation("Like")
{
    protected override bool Evaluate(object? input, object value)
    {
        if (value.ToString() is not { } pattern) return false;
        if (input?.ToString() is not { } str) return false;

        var regex = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
        return Regex.IsMatch(str, regex);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}