namespace AutoSpex.Engine.Operations;

public abstract class BinaryOperation : Operation
{
    protected BinaryOperation(string name) : base(name)
    {
    }

    public override bool Execute(object? input, params object[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (values.Length != 1)
            throw new ArgumentException("Binary operations require exactly one value", nameof(values));
        
        return Evaluate(input, values[0]);
    }

    protected abstract bool Evaluate(object? input, object value);
    
    public override int NumberOfArguments => 1;
}