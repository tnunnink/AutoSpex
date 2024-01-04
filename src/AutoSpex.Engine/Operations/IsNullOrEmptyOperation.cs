namespace AutoSpex.Engine.Operations;

public class IsNullOrEmptyOperation : UnaryOperation
{
    public IsNullOrEmptyOperation() : base("Is Null Or Empty")
    {
    }

    protected override bool Evaluate(object? input)
    {
        return string.IsNullOrEmpty(input?.ToString());
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}