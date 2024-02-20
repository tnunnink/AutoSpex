namespace AutoSpex.Engine;

public class IsTrueOperation() : UnaryOperation("Is True")
{
    public override string ShouldMessage => "Should Be True";

    protected override bool Evaluate(object? input) => input is true;

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Boolean;
}