namespace AutoSpex.Engine.Operations;

public abstract class TernaryOperation : Operation
{
    protected TernaryOperation(string name) : base(name)
    {
    }

    public override bool Execute(object? input, params object[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length != 2)
            throw new ArgumentException("Ternary operations require exactly two values", nameof(values));
        
        return Evaluate(input, values[0], values[1]);
    }
    
    protected abstract bool Evaluate(object? input, object first, object second);
}