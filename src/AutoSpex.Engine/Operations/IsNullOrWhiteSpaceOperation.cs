namespace AutoSpex.Engine.Operations;

public class IsNullOrWhiteSpaceOperation : UnaryOperation
{
    public IsNullOrWhiteSpaceOperation() : base("Is Null Or WhiteSpace")
    {
    }

    protected override bool Evaluate(object? input)
    {
        return string.IsNullOrWhiteSpace(input?.ToString());
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}