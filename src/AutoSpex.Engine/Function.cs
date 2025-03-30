using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

public abstract class Function(string name, string value) : SmartEnum<Function, string>(name, value)
{
    
    public abstract bool Invoke(object? value);
}