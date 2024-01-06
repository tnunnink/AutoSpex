using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Argument
{
    public Argument(object value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        Type = value.GetType();
        Value = value;
    }
    
    public Type Type { get; }
    public object Value { get; }
    public TypeGroup Group => TypeGroup.FromType(Type);
    
    public IEnumerable<object> Options => Type.GetOptions();

    public static implicit operator Argument(ValueType value) => new(value);

    public static implicit operator Argument(AtomicType value) => new(value);

    public static implicit operator Argument(string value) => new(value);
    
    public static implicit operator Argument(DateTime value) => new(value);

    public static implicit operator Argument(Enum value) => new(value);

    public static implicit operator Argument(LogixEnum value) => new(value);

    public static implicit operator Argument(Criterion value) => new(value);

    public static implicit operator Argument(Operation value) => new(value);
    
    public static implicit operator Argument(Variable value) => new(value);
}