namespace AutoSpex.Engine;

public class VoidOperation() : UnaryOperation("Void")
{
    protected override bool Evaluate(object? input)
    {
        if (input is null) return true;
        if (input is not string s) return false;
        return string.IsNullOrWhiteSpace(s);
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}