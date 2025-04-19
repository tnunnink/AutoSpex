using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a simple named value reference to an object which can be plugged into an argument for a criterion
/// so that the user can later define or override the arguments for a given criterion on a spec.
/// </summary>
public class Variable
{
    /// <summary>
    /// Creates a new <see cref="Variable"/> with default empty values.
    /// </summary>
    public Variable()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the provided type group.
    /// </summary>
    /// <param name="group">The <see cref="TypeGroup"/> of the variable.</param>
    /// <exception cref="ArgumentNullException"><paramref name="group"/> is <c>null</c>.</exception>
    public Variable(TypeGroup group)
    {
        Group = group ?? throw new ArgumentNullException(nameof(group));
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the provided name an optional value object.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    public Variable(string name, object? value = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Can not create variable with null or empty name.");

        Name = name;
        Value = value;
        Group = TypeGroup.FromType(value?.GetType());
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> from a provided scope logix element, using the scope path as the name
    /// and the object instance as the value.
    /// </summary>
    public Variable(LogixScoped component)
    {
        ArgumentNullException.ThrowIfNull(component);

        Name = component.Scope.Path;
        Value = component;
        Group = TypeGroup.Element;
    }

    /// <summary>
    /// The unique id of the <see cref="Variable"/> object.
    /// </summary>
    [JsonInclude]
    public Guid VariableId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The <see cref="Guid"/> that identifies the node this variable is defined for.
    /// By default, this is an empty guid, but should be set upon retrieval from the database so that the object can
    /// identify which node it is scoped to.
    /// </summary>
    [JsonIgnore]
    public Guid NodeId { get; private set; } = Guid.Empty;

    /// <summary>
    /// The string name used to identify the <see cref="Variable"/>.
    /// </summary>
    [JsonInclude]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The <see cref="TypeGroup"/> which the variable value belongs. This is somewhat loosely coupled to the value type
    /// since you can change value and not group. But this is more for the interface so the user can select a group which
    /// we can use to attempt to parse their text input.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<TypeGroup, int>))]
    [JsonInclude]
    public TypeGroup Group { get; set; } = TypeGroup.Text;

    /// <summary>
    /// The object value of the <see cref="Variable"/>.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    [JsonInclude]
    public object? Value { get; set; }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    public static implicit operator Reference(Variable variable) => new(variable.Name);
}