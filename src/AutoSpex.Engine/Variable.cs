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
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name can not be null or empty");
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Variable value can not be null or empty");

        Name = name;
        Value = value;
        Description = description;
    }
    
    public Guid VariableId { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}