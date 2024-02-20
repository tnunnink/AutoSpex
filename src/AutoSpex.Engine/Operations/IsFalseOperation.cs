namespace AutoSpex.Engine;

public class IsFalseOperation() : UnaryOperation("Is False")
{
    public override string ShouldMessage => "Should Be False";

    protected override bool Evaluate(object? input) => input is false;

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Boolean;
}