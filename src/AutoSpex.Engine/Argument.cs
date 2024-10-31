using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Argument
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
    /// The value of the argument, which is just a generic object, since the user can enter primitive or complex types.
    /// This value is persisted and materialized using a custom JSON serializer.
    /// </summary>
    [JsonConverter(typeof(JsonObjectConverter))]
    [JsonInclude]
    public object? Value { get; set; }

    /// <summary>
    /// Gets the default Argument object with null value.
    /// </summary>
    public static Argument Default => new(default);

    /// <summary>
    /// Resolves the underlying argument value to the specified type if possible.
    /// </summary>
    /// <param name="type">The type for which to resolve or convert to.</param>
    /// <returns>An object value of the specified type.</returns>
    /// <remarks>
    /// </remarks>
    public object ResolveAs(Type? type)
    {
        var value = Value;

        var typed = value switch
        {
            Criterion criterion => criterion,
            IEnumerable<Argument> arguments => arguments.Select(a => a.ResolveAs(type)).ToList(),
            string text when type is not null && type != typeof(string) => text.TryParse(type),
            //todo this needs te thought through more I think. We might not want to alwasy convert if we don't have to
            not string when type is not null && type != value?.GetType() && value is IConvertible convertible =>
                convertible.ToType(type, null),
            _ => value
        };

        return (typed ?? value)!;
    }

    /// <summary>
    /// Traverses the argument value and retrieves the final expected argument value(s).
    /// Since an argument value can be a nested <see cref="Criterion"/> or collection of <see cref="Argument"/> values,
    /// we want to check them and get the values which are going to be used in the operation.
    /// </summary>
    /// <returns>A collection of object values that represent the final arguments.</returns>
    public IEnumerable<object> Expected()
    {
        return Value switch
        {
            Criterion criterion => criterion.Argument.Expected(),
            IEnumerable<Argument> arguments => arguments.SelectMany(a => a.Expected()),
            _ => Value is not null ? [Value] : []
        };
    }

    /// <summary>
    /// Creates a new <see cref="Argument"/> with the same value as the current instance.
    /// </summary>
    /// <returns>A new <see cref="Argument"/> instance with the same value.</returns>
    public Argument Duplicate() => new(Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToText();

    public static implicit operator Argument(ValueType value) => new(value);
    public static implicit operator Argument(string value) => new(value);
    public static implicit operator Argument(DateTime value) => new(value);
    public static implicit operator Argument(LogixEnum value) => new(value);
    public static implicit operator Argument(LogixElement value) => new(value);
    public static implicit operator Argument(List<Argument> value) => new(value);
    public static implicit operator Argument(List<object> value) => new(value);
    public static implicit operator Argument(Criterion value) => new(value);
}