using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Argument
{
    public Argument(object value)
    {
        if (value is string s && string.IsNullOrEmpty(s))
            throw new ArgumentException("Can not set argument to null or empty value");
        
        Value = value ?? throw new ArgumentNullException(nameof(value), "Can not set argument to null value");
    }
    
    public object Value { get; }
    public Type Type => Value.GetType();
    public TypeGroup Group => TypeGroup.FromType(Type);

    /// <summary>
    /// Resolves the underlying argument value to the specified type if applicable.
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

        //If a criterion was provided, just return that. Nested arguments will return get resolved here too.
        if (value is Criterion criterion)
            return criterion;

        //From here we expect some immediate value. If the type is not specified the only option is to
        //return what we have. This may result in an error but this will tell the user something is wrong.
        if (type is null)
            return value;

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
    
    public static implicit operator Argument(ValueType value) => new(value);
    public static implicit operator Argument(string value) => new(value);
    public static implicit operator Argument(DateTime value) => new(value);
    public static implicit operator Argument(LogixEnum value) => new(value);
    public static implicit operator Argument(Criterion value) => new(value);
    public static implicit operator Argument(Variable value) => new(value);
}