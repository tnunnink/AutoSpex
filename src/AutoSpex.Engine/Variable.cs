using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a simple named value reference to an object which can be plugged into an argument for a criterion
/// so that the user can later define or override the arguments for a given criterion on a spec.
/// </summary>
public class Variable : IEquatable<Variable>
{
    /// <summary>
    /// Creates a new <see cref="Variable"/> with default empty values.
    /// </summary>
    public Variable()
    {
        ChangeGroup(TypeGroup.Text);
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> having the provided node id.
    /// </summary>
    /// <param name="nodeId">The id of the node this variable belongs to.</param>
    public Variable(Guid nodeId) : this()
    {
        NodeId = nodeId;
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the provided type group.
    /// </summary>
    /// <param name="group">The <see cref="TypeGroup"/> of the variable.</param>
    /// <exception cref="ArgumentNullException"><paramref name="group"/> is <c>null</c>.</exception>
    public Variable(TypeGroup group)
    {
        ChangeGroup(group);
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the provided name an optional value object.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    public Variable(string name, object? value = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Can not create variable with null or empty name.");

        Name = name;
        Value = value;
        Group = TypeGroup.FromType(value?.GetType());
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
    [JsonInclude]
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
    public TypeGroup Group { get; private set; } = TypeGroup.Default;

    /// <summary>
    /// The object value of the <see cref="Variable"/>.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    [JsonInclude]
    public object? Value { get; set; }

    /// <summary>
    /// The type of the variable value.
    /// </summary>
    [JsonIgnore]
    public Type? Type => Value?.GetType();

    /// <summary>
    /// The <see cref="Engine.Reference"/> object that refers to the name of this Variable.
    /// </summary>
    /// <value>A <see cref="Engine.Reference"/> with the name of this variable.</value>
    [JsonIgnore]
    public Reference Reference => new(Name, Value);

    /// <summary>
    /// Updates the variable <see cref="Group"/> that the value represents.
    /// </summary>
    /// <param name="group">A <see cref="TypeGroup"/> type.</param>
    /// <remarks>This also resets <see cref="Value"/> and <see cref="Type"/> based on the default value for the provided group.</remarks>
    public void ChangeGroup(TypeGroup group)
    {
        Group = group ?? throw new ArgumentNullException(nameof(group));
        Value = group.DefaultValue;
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <inheritdoc />
    public bool Equals(Variable? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || VariableId.Equals(other.VariableId);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Variable other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => VariableId.GetHashCode();

    public static implicit operator Reference(Variable variable) => new(variable.Name);
}