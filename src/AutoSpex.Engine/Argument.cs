using L5Sharp.Core;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace AutoSpex.Engine;

public class Argument : IEquatable<Argument>
{
    /// <summary>
    /// Creates a new <see cref="Argument"/> with the empty string default value.
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
    /// Creates a new argument with the provided object value. 
    /// </summary>
    /// <param name="argumentId">The guid identifying the argument instance.</param>
    /// <param name="value">The value of the argument.</param>
    public Argument(Guid argumentId, object? value)
    {
        ArgumentId = argumentId;
        Value = value;
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this object.
    /// </summary>
    public Guid ArgumentId { get; } = Guid.NewGuid();

    /// <summary>
    /// The value of the argument.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The type of the argument's value.
    /// </summary>
    public Type? Type => Value?.GetType();

    /// <summary>
    /// The friendly type name of the argument value.
    /// </summary>
    public string? Identifier => Type?.CommonName();

    /// <summary>
    /// The <see cref="TypeGroup"/> which this argument value belongs to.
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Type);

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
        //If a variable was provided, take the inner variable value, otherwise take this value.
        var value = Value is Variable variable ? variable.Value : Value;

        //If a criterion was provided, just return that. Nested arguments will get resolved here too.
        if (value is Criterion criterion) return criterion;

        //From here we expect some immediate value.
        //If the type is not specified or value is null or not text, the only option is to return what we have.
        //Exceptions will get caught in evaluation, so we don't need to worry about null reference.
        if (type is null || value is not string text) return value!;

        //Text is a special case because the user may enter value as text, and we can attempt to parse it to get
        //the equality methods to work as expected. If not parsed then return value.
        return text.TryParse(type) ?? value;
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
    public static implicit operator Argument(Criterion value) => new(value);
    public static implicit operator Argument(Variable value) => new(value);
}