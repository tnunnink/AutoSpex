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
    public object Resolve(Type? type)
    {
        //If a variable was provided, use the inner variable value.
        var value = Value is Variable variable ? variable.Value : Value;

        //If a criterion was provided, just return that.
        if (value is Criterion criterion)
            return criterion;

        //From here we expect some immediate value. If the type is not specified the only option is to
        //return what we have. This may result in an error but this will tell the user something is wrong.
        if (type is null)
            return value;

        //If this is a string argument but the type needs to be parsed, we need to attempt that here.
        if (value is string text && type != typeof(string))
        {
            return text.Parse(type); //todo at some point we want to TryParse the text and if failed just return value.
        }
        
        return value;
    }
    
    public static implicit operator Argument(ValueType value) => new(value);
    public static implicit operator Argument(string value) => new(value);
    public static implicit operator Argument(DateTime value) => new(value);
    public static implicit operator Argument(LogixEnum value) => new(value);
    public static implicit operator Argument(Criterion value) => new(value);
    public static implicit operator Argument(Variable value) => new(value);
}