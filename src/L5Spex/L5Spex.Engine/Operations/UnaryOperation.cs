namespace L5Spex.Engine.Operations;

public abstract class UnaryOperation : Operation
{
    protected UnaryOperation(string name) : base(name)
    {
    }
    
    public override bool Evaluate(object? input, params object[] values) => Evaluate(input);

    protected abstract bool Evaluate(object? input);
}