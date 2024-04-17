namespace AutoSpex.Engine;

public abstract class TernaryOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, params object[] values)
    {
        if (values is null) throw new ArgumentNullException(nameof(values));
        
        if (values.Length != 2)
            throw new ArgumentException("Ternary operations require exactly two values", nameof(values));
        
        return Evaluate(input, values[0], values[1]);
    }
    
    protected abstract bool Evaluate(object? input, object first, object second);
    
    public override int NumberOfArguments => 2;
}