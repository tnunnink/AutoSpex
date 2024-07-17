namespace AutoSpex.Engine;

public class FalseOperation() : UnaryOperation("False")
{
    protected override bool Evaluate(object? input) => input is false;

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Boolean;
}