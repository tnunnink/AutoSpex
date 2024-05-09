using System.Xml.Linq;
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
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> assigned to the provided node id.
    /// </summary>
    public Variable(Guid nodeId)
    {
        NodeId = nodeId;
    }

    /// <summary>
    /// Creates a new <see cref="Variable"/> with the provided name an optional value object.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <param name="description">The optional description of the variable.</param>
    public Variable(string name, object? value = default, string? description = default)
    {
        Name = name;
        Value = value ?? string.Empty;
        Description = description ?? string.Empty;
    }

    public Variable(Guid nodeId, string name, object? value = default, string? description = default)
    {
        NodeId = nodeId;
        Name = name;
        Value = value ?? string.Empty;
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// The unique id of the <see cref="Variable"/> object.
    /// </summary>
    public Guid VariableId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The node id that defines this <see cref="Variable"/> object.
    /// </summary>
    public Guid NodeId { get; private set; } = Guid.Empty;

    /// <summary>
    /// The string name used to identify the <see cref="Variable"/>.
    /// </summary>
    public string Name { get; set; } = "Variable";

    /// <summary>
    /// The type of the variable value. This is persisted, so we know how to materialize the object value to a strongly
    /// typed object.
    /// </summary>
    public Type Type { get; private set; } = typeof(string);

    /// <summary>
    /// The raw string data representing the value of the variable. This is what is persisted and ultimately what
    /// <see cref="Value"/> is after being parsed using the defined type.
    /// </summary>
    /// <remarks>
    /// By default, this will be an empty string making it a simple text type variable in which the user can
    /// assign a value. However, this can also be an inner variable for which to chain to another value.
    /// </remarks>
    public string Data { get; private set; } = string.Empty;

    /// <summary>
    /// The object value of the <see cref="Variable"/>.
    /// </summary>
    /// <remarks>
    /// By default, this will be an empty string making it a simple text type variable in which the user can
    /// assign a value. However, this can also be a complex object like a LogixElement or LogixEnum value.
    /// </remarks>
    public object Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    /// <summary>
    /// A summary of what the variable represents or is used for.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The object override value for the <see cref="Variable"/>.
    /// </summary>
    /// <remarks>
    /// The user can "override" variable values to test different value or change them depending on the use within a Runner.
    /// This value is persisted in a different table but attached here. We will use <see cref="ResolveValue"/> to determine
    /// which value to use for a given criterion/spec. This value is null by default (meaning no override).
    /// </remarks>
    public object? Override
    {
        get => GetOverride();
        set => SetOverride(value);
    }

    /// <summary>
    /// The raw string data representing the override value for the variable if set. This is persisted same
    /// as <see cref="Data"/> and will be parsed using the same get/set methods to retrieve the actual object value.
    /// </summary>
    public string? OverrideData { get; private set; }

    /// <summary>
    /// The type group which the variable value belongs.
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Type);

    /// <summary>
    /// A formatted representation of the variable name which is wrapping the name in braces. This is how the user
    /// references variables using the brace syntax.
    /// </summary>
    public string Formatted => $"{VariableStart}{Name}{VariableEnd}";

    /// <inheritdoc />
    public override string ToString() => Formatted;

    /// <summary>
    /// Returns the final value of the <see cref="Variable"/> object, which is either the override value of the actual
    /// assigned value of the variable.
    /// </summary>
    /// <returns>An object representing the referenced value of the variable.</returns>
    public object ResolveValue() => Override ?? Value;

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
    /// Gets the strongly typed object value using the variable's current <see cref="Data"/> and <see cref="Type"/>.
    /// </summary>
    private object GetValue() => ParseData(Data, Type);

    /// <summary>
    /// Sets the <see cref="Data"/> and <see cref="Type"/> of the variable with the provided object value.
    /// </summary>
    private void SetValue(object? value)
    {
        Data = StringifyData(value);
        Type = value?.GetType() ?? typeof(string);
    }

    /// <summary>
    /// Gets the strongly typed object override using the variable's current <see cref="OverrideData"/> and <see cref="Type"/>.
    /// </summary>
    private object? GetOverride() => OverrideData is not null ? ParseData(OverrideData, Type) : default;

    /// <summary>
    /// Sets the <see cref="OverrideData"/> for the variable with the provided object value.
    /// </summary>
    private void SetOverride(object? value) => OverrideData = StringifyData(value);

    /// <summary>
    /// Gets an object value using the provided string data and specified type information.
    /// </summary>
    private static object ParseData(string data, Type type)
    {
        //If the type is a simple string just return the Data.
        if (type == typeof(string))
            return data;

        //If this is a logix element type, then we will deserialize it.
        if (type.IsAssignableTo(typeof(LogixElement)))
            return XElement.Parse(data).Deserialize();

        //If L5Sharp can parse the type then we use its parser. Anything not parsable or known will just be text.
        return type.IsParsable() ? data.Parse(type) : data;
    }

    /// <summary>
    /// Converts an input object to the storable string value which we will know how to convert back to the
    /// strongly typed object.
    /// </summary>
    private static string StringifyData(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string v => v,
            LogixElement v => v.Serialize().ToString(),
            LogixEnum v => v.Name,
            ValueType v => v.ToString()!,
            _ => throw new NotSupportedException($"Value {value.GetType()} is not a supported variable value type.")
        };
    }
}