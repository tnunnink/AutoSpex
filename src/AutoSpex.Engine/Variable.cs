using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a simple named value reference to an object which can be plugged into an argument for a criterion
/// so that the user can later define or override the arguments for a given criterion on a spec.
/// </summary>
public class Variable : IEquatable<Variable>
{
    private const char VariableStart = '{';
    private const char VariableEnd = '}';

    /// <summary>
    /// Creates a new <see cref="Variable"/> with default empty values.
    /// </summary>
    public Variable()
    {
        ChangeGroup(TypeGroup.Text);
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
        Group = TypeGroup.FromType(value?.GetType());
        Type = value?.GetType() ?? Group.DefaultType;
        Value = value;
    }

    /// <summary>
    /// The unique id of the <see cref="Variable"/> object.
    /// </summary>
    public Guid VariableId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The string name used to identify the <see cref="Variable"/>.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The qualified path representing the node this variable belongs to.
    /// </summary>
    public string Path { get; private set; } = string.Empty;

    /// <summary>
    /// Represents a scoped reference to a <see cref="Variable"/> object.
    /// </summary>
    public string ScopedReference => string.Concat(VariableStart, Name, VariableEnd);

    /// <summary>
    /// Represents the absolute reference of a <see cref="Variable"/> object.
    /// </summary>
    /// <remarks>
    /// The absolute reference is the full path of a variable, including its name and the path of its parent object, if any.
    /// It is used to uniquely identify a variable within a system.
    /// </remarks>
    public string AbsoluteReference =>
        string.IsNullOrEmpty(Path) ? ScopedReference : string.Concat(VariableStart, Path, ".", Name, VariableEnd);

    /// <summary>
    /// The <see cref="TypeGroup"/> which the variable value belongs.
    /// </summary>
    public TypeGroup Group { get; private set; } = TypeGroup.Default;

    /// <summary>
    /// The type of the variable value. This is persisted, so we know how to materialize the object value to a strongly
    /// typed object at runtime, which will allow us to pass in strongly typed values for criteria evaluation.
    /// </summary>
    public Type? Type { get; private set; } = typeof(object);

    /// <summary>
    /// The object value of the <see cref="Variable"/>.
    /// </summary>
    public object? Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    /// <summary>
    /// The raw string data representing the value of the variable. This is what is persisted and ultimately what
    /// <see cref="Value"/> is after being parsed using the defined type.
    /// </summary>
    public string? Data { get; private set; }

    /// <summary>
    /// Gets or sets the description of the <see cref="Variable"/> object.
    /// </summary>
    public string? Description { get; set; }

    /// <inheritdoc />
    public override string ToString() => ScopedReference;

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

    /// <summary>
    /// Gets an object value using the provided string data and specified type information.
    /// </summary>
    private object? GetValue()
    {
        if (Data is null || Type is null)
            return default;

        //Technically out LogixParser can handle all the types we care about.
        return Type.IsParsable() ? Data.TryParse(Type) : null;
    }

    /// <summary>
    /// Converts an input object to the storable string  and assigns to <see cref="Data"/>,
    /// which we will know how to convert back to the strongly typed object when retrieving from the database.
    /// </summary>
    private void SetValue(object? value)
    {
        Type = value?.GetType();
        //todo handling collections that are not LogixContainer
        Data = value switch
        {
            null => null,
            string v => v,
            LogixEnum v => v.Name,
            AtomicData v => v.ToString(),
            LogixElement v => v.Serialize().ToString(),
            _ => value.ToString()
        };
    }
}