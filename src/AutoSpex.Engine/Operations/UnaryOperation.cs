namespace AutoSpex.Engine.Operations;

public abstract class UnaryOperation : Operation
{
    protected UnaryOperation(string name) : base(name)
    {
    }
    
    public override bool Execute(object? input, params object[] values) => Evaluate(input);

    protected abstract bool Evaluate(object? input);

    public override int NumberOfArguments => 0;
}