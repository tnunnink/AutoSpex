namespace AutoSpex.Engine;

public abstract class BinaryOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, object? value)
    {
        if (value is null) 
            throw new ArgumentNullException(nameof(value), "Binary operations expect an argument.");

        return Evaluate(input, value);
    }

    protected abstract bool Evaluate(object? input, object value);
}