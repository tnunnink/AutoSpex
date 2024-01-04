using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Arg
{
    public Arg(object value)
    {
        Type = value.GetType();
        Group = TypeGroup.FromType(Type);
        Value = value;
    }
    
    public Type Type { get; }
    public TypeGroup Group { get; }
    public object Value { get; }
    
    public static implicit operator Arg(ValueType value) => new(value);
    
    public static implicit operator Arg(AtomicType value) => new(value);

    public static implicit operator Arg(string value) => new(value);

    public static implicit operator Arg(Enum value) => new(value);
    
    public static implicit operator Arg(DateTime value) => new(value);
    
    public static implicit operator Arg(LogixEnum value) => new(value);
    
    public static implicit operator Arg(Criterion value) => new(value);
}