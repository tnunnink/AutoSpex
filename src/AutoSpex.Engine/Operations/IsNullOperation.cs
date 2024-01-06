namespace AutoSpex.Engine;

public class IsNullOperation() : UnaryOperation("Is Null")
{
    protected override bool Evaluate(object? input) => input is null;
}