namespace AutoSpex.Engine;

public abstract class BinaryOperation(string name) : Operation(name)
{
    public override bool Execute(object? input, object? value = default)
    {
        if (value is null)
            throw new ArgumentException($"Argument value required for {Name} operation.");

        return Evaluate(input, value);
    }

    protected abstract bool Evaluate(object? input, object value);
}