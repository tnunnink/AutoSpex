using JetBrains.Annotations;

namespace AutoSpex.Engine;

public class Variable
{
    [UsedImplicitly]
    private Variable()
    {
    }

    public Variable(string name, string value, string? description = default)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Description = description;
    }

    public Guid VariableId { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public override string ToString() => Name;
}