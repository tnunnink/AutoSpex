namespace AutoSpex.Engine;

public class IsNullOrEmptyOperation() : UnaryOperation("Is Null Or Empty")
{
    public override string ShouldMessage => "Should Be Null Or Empty";

    protected override bool Evaluate(object? input)
    {
        return string.IsNullOrEmpty(input?.ToString());
    }

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Text;
}