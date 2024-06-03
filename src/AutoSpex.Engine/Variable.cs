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
    private Type _type = typeof(object);

    /// <summary>
    /// Creates a new <see cref="Variable"/> with default empty values.
    /// </summary>
    public Variable()
    {
        Value = TypeGroup.Text.DefaultValue;
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the provided type group.
    /// </summary>
    /// <param name="group">The <see cref="TypeGroup"/> of the variable.</param>
    /// <exception cref="ArgumentNullException"><paramref name="group"/> is <c>null</c>.</exception>
    public Variable(TypeGroup group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        if (group.DefaultValue is not null)
        {
            //This will also Type and in turn set Group
            Value = group.DefaultValue;
            return;
        }

        //This will also set Group.
        Type = group.DefaultType;
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
    /// The type of the variable value. This is persisted, so we know how to materialize the object value to a strongly
    /// typed object at runtime, which will allow us to pass in strongly typed values for criteria evaluation.
    /// Whenever we set type, we will also set the <see cref="Group"/> to match the type group for this variable.
    /// </summary>
    public Type Type
    {
        get => _type;
        private set
        {
            _type = value;
            Group = TypeGroup.FromType(value);
        }
    }

    /// <summary>
    /// The <see cref="TypeGroup"/> which the variable value belongs.
    /// </summary>
    public TypeGroup Group { get; private set; } = TypeGroup.Default;

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
    /// <remarks>
    /// By default, this will be an empty string making it a simple text type variable in which the user can
    /// assign a value. However, this can also be an inner variable for which to chain to another value.
    /// </remarks>
    public string? Data { get; private set; }

    /// <summary>
    /// Gets or sets the description of the <see cref="Variable"/> object.
    /// </summary>
    public string? Description { get; set; }

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

    /// <summary>
    /// Updates the variable <see cref="Group"/> that the value represents.
    /// </summary>
    /// <param name="group">A <see cref="TypeGroup"/> type.</param>
    public void ChangeType(TypeGroup group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        Type = group.DefaultType;
        Value = group.DefaultValue;
    }

    /// <summary>
    /// Gets an object value using the provided string data and specified type information.
    /// </summary>
    private object? GetValue()
    {
        if (Data is null || Type == typeof(object)) return default;

        if (Type == typeof(string))
            return Data;

        return Type.IsParsable() ? Data.TryParse(Type) : null;
    }

    /// <summary>
    /// Converts an input object to the storable string  and assigns to <see cref="Data "/>,
    /// which we will know how to convert back to the strongly typed object when retrieving from the database.
    /// </summary>
    private void SetValue(object? value)
    {
        //Only update type if the provided value is not null.
        //We want to maintain the specified target type unless the user provides a new valid type.
        if (value is not null)
        {
            Type = value.GetType();
        }

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