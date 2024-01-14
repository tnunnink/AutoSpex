namespace AutoSpex.Engine;

public class IsNullOperation() : UnaryOperation("Is Null")
{
    public override string ShouldMessage => "Should Be Null";
    protected override bool Evaluate(object? input) => input is null;
}