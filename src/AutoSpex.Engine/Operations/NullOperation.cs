namespace AutoSpex.Engine;

public class NullOperation() : UnaryOperation("Null")
{
    protected override bool Evaluate(object? input) => input is null;
}