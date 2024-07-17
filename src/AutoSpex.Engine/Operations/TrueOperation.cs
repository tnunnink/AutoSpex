namespace AutoSpex.Engine;

public class TrueOperation() : UnaryOperation("True")
{
    protected override bool Evaluate(object? input) => input is true;

    protected override bool Supports(TypeGroup group) => group == TypeGroup.Boolean;
}