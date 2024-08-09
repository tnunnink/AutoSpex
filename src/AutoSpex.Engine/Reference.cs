using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A more lean class that represents a reference to a variable. We don't want to persist all the variable information
/// when it is set on an argument of a criterion. We want to just hold the name of the variable that we can then resolve
/// at runtime. This aleviates data consistency issues between variables and arguments referencing them.
/// </summary>
public class Reference
{
    /// <summary>
    /// The prefix that identifies a vlaue as a reference to a variable.
    /// </summary>
    public const char Prefix = '@';
    
    /// <summary>
    /// Creates a new <see cref="Reference"/> instance with the provided name and optional value.
    /// </summary>
    /// <param name="name">The name of the reference that corresponds to a scoped variable name.</param>
    /// <param name="value">The optional value to use as the reference value.</param>
    /// <exception cref="ArgumentNullException"><c>name</c> is null.</exception>
    public Reference(string name, object? value = default)
    {
        Name = name.TrimStart(Prefix) ?? throw new ArgumentNullException(nameof(name));
        Value = value;
    }

    /// <summary>
    /// The unique id of the reference intance.
    /// </summary>
    [JsonInclude]
    public Guid ReferenceId { get; private init; } = Guid.NewGuid();
    
    /// <summary>
    /// The name of the variable that this reference represents.
    /// </summary>
    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The resolved value of the reference. This is not perisited and only set when resolving references to scoped
    /// variables, or when initially created using an existing variable.
    /// </summary>
    [JsonIgnore]
    public object? Value { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{Prefix}{Name}";
}