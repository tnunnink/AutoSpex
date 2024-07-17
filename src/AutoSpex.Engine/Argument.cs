using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Argument : IEquatable<Argument>
{
    /// <summary>
    /// Creates a new <see cref="Argument"/> with the null value.
    /// </summary>
    public Argument()
    {
    }

    /// <summary>
    /// Creates a new argument with the provided object value. 
    /// </summary>
    /// <param name="value">The value of the argument.</param>
    public Argument(object? value)
    {
        Value = value;
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this object.
    /// </summary>
    [JsonInclude]
    public Guid ArgumentId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The value of the argument, which is just a generic object, since the user can enter primitive or complex types.
    /// This value is persisted and materialized using a custom JSON serializer.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    [JsonInclude]
    public object? Value { get; set; }

    /// <summary>
    /// Resolves the underlying argument value to the specified type if possible.
    /// </summary>
    /// <param name="type">The type for which to resolve or convert to.</param>
    /// <returns>An object value of the specified type.</returns>
    /// <remarks>
    /// This method if detect <see cref="Variable"/> type and use the inner value first. If <see cref="Value"/>
    /// is actually a<see cref="Criterion"/>  will return that regardless of provided type. If type is null we
    /// will return whatever value we have. And finally this method will attempt to parse the value if it is a string
    /// and the specified type is something other than a string type.
    /// </remarks>
    public object ResolveAs(Type? type)
    {
        //If a variable is configured, take the inner variable value, otherwise take this literal value.
        var value = Value is Variable variable ? variable.Value : Value;

        //If a criterion was provided, just return that. Nested arguments will get resolved here too.
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (value is Criterion criterion)
        {
            return criterion;
        }

        //Text is a special case because the user may enter value as text, and we can attempt to parse it to get
        //the equality methods to work as expected. If not parsed then return value.
        if (value is string text && type is not null && type != typeof(string))
        {
            return text.TryParse(type) ?? value;
        }

        //If this is a typed value that is convertible, convert it.
        if (value is not string && value is IConvertible convertible && type is not null)
        {
            return convertible.ToType(type, null);
        }

        //Just return what we have and if it fails we will catch the exception in the criterion and display the error
        return value!;
    }

    /// <summary>
    /// Traverses the argument value and retrieves the final expected argument value(s).
    /// Since an argument value can be a <see cref="Variable"/> or inner <see cref="Criterion"/> we want to check
    /// them and get the values which are going to be used in the operation.
    /// </summary>
    /// <returns>A collection of object values that represent the final arguments.</returns>
    public IEnumerable<object> Expected()
    {
        var value = Value is Variable variable ? variable.Value : Value;

        if (value is Criterion criterion)
            return criterion.Arguments.SelectMany(a => a.Expected());

        return value is not null ? [value] : Enumerable.Empty<object>();
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToText();

    /// <inheritdoc />
    public bool Equals(Argument? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || ArgumentId.Equals(other.ArgumentId);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Argument other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => ArgumentId.GetHashCode();

    public static implicit operator Argument(ValueType value) => new(value);
    public static implicit operator Argument(string value) => new(value);
    public static implicit operator Argument(DateTime value) => new(value);
    public static implicit operator Argument(LogixEnum value) => new(value);
    public static implicit operator Argument(LogixElement value) => new(value);
    public static implicit operator Argument(Criterion value) => new(value);
    public static implicit operator Argument(Variable value) => new(value);
}