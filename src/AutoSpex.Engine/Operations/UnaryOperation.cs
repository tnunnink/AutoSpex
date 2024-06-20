namespace AutoSpex.Engine;

public abstract class UnaryOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, params object[] values) => Evaluate(input);

    protected abstract bool Evaluate(object? input);
}