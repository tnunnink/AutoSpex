namespace AutoSpex.Engine;

public class Variable
{
    private const char VariableStart = '{';
    private const char VariableEnd = '}';

    public Variable()
    {
    }

    public Variable(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Variable(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
    }

    public Guid VariableId { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Type Type => Value?.GetType();
    public string? Value { get; set; } = string.Empty;
    public string? Override { get; set; } = string.Empty;
    public string Formatted => $"{VariableStart}{Name}{VariableEnd}";
    public override string ToString() => Formatted;
}