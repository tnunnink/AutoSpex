using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Argument
{
    /// <summary>
    /// Creates a new <see cref="Argument"/> with the empty string default value.
    /// </summary>
    public Argument()
    {
        Value = string.Empty;
    }

    /// <summary>
    /// Creates a new argument with the provided object value. 
    /// </summary>
    /// <param name="value">The value of the argument.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public Argument(object value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value), "Can not set argument to null value");
    }
    
    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this object.
    /// </summary>
    public Guid ArgumentId { get; } = Guid.NewGuid();

    /// <summary>
    /// The value of the argument.
    /// </summary>
    public object Value { get; private set; }
    
    /// <summary>
    /// The type of the argument value.
    /// </summary>
    public Type Type => Value.GetType();
    
    /// <summary>
    /// The friendly type name of the argument value.
    /// </summary>
    public string Identifier => Type.TypeIdentifier();
    
    /// <summary>
    /// The <see cref="TypeGroup"/> to which this argument value belongs.
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Type);

    /// <summary>
    /// Sets the <see cref="Value"/> of the argument. If the provided value is null this will set value to an empty string
    /// to ensure there is no null reference exceptions for this type.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetValue(object? value)
    {
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Resolves the underlying argument value to the specified type if possible.
    /// </summary>
    /// <param name="type">The type for which to resolve or convert to.</param>
    /// <returns>An object value of the specified type.</returns>
    /// <remarks>
    /// This method if detect <see cref="Variable"/> type and use the inner value first. If <see cref="Value"/>
    /// is actually a <see cref="Criterion"/> it will return that regardless of provided type. If type is null we
    /// will return whatever value we have. And finally this method will attempt to parse the value if it is a string
    /// and the specified type is something other than a string type.
    /// </remarks>
    public object ResolveAs(Type? type)
    {
        //If a variable was provided, take the inner variable value, otherwise take this value.
        var value = Value is Variable variable ? variable.Value : Value;

        //If a criterion was provided, just return that. Nested arguments will get resolved here too.
        if (value is Criterion criterion)
            return criterion;

        //From here we expect some immediate value. If the type is not specified the only option is to
        //return what we have. This may result in an error but this will tell the user something is wrong.
        if (type is null) return value;

        //If this is is not a string or we are simply trying to resolve it as one, just return that.
        if (value is not string text || type == typeof(string)) return value;

        //If not parsable by L5Sharp then just return.
        if (!type.IsParsable()) return value;

        //Otherwise we want to attempt to parse the string to the specified typed value if possible in order
        //to use that types defined equality overrides. This is using a built in method from L5Sharp which
        //knows how to parse it's types (as well as primitive types).
        var parsed = text.TryParse(type);
        if (parsed is not null) return parsed;

        //And if none of that returned, just return the value.
        return value;
    }

    public override string? ToString() => Value.ToString();

    public static Argument Empty => new();
    public static implicit operator Argument(ValueType value) => new(value);
    public static implicit operator Argument(string value) => new(value);
    public static implicit operator Argument(DateTime value) => new(value);
    public static implicit operator Argument(LogixEnum value) => new(value);
    public static implicit operator Argument(Criterion value) => new(value);
    public static implicit operator Argument(Variable value) => new(value);
}