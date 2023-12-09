using System.Collections;
using System.Reflection;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Property
{
    public Property(string name, Type type)
    {
        Name = name;
        Type = type;
        Group = TypeGroup.FromType(Type);
    }
    
    public Property(PropertyInfo info)
    {
        Name = info.Name;
        Type = info.PropertyType;
        Group = TypeGroup.FromType(Type);
    }

    public string Name { get; }

    public Type Type { get; }

    public TypeGroup Group { get; }

    public Property? Nested(string name)
    {
        var index = name.IndexOf('.');
        var member = index >= 0 ? name[..index] : name;
        var info = Type.GetProperties().FirstOrDefault(p => p.Name == member);
        var property = info is not null ? new Property(info) : default;
        return index < 0 ? property : property?.Nested(name[(index + 1)..]);
    }

    public bool IsCollection => typeof(IEnumerable).IsAssignableFrom(Type) && Type != typeof(string);

    public bool IsLogixElement => typeof(LogixElement).IsAssignableFrom(Type);
    
    public bool IsLogixType => typeof(LogixType).IsAssignableFrom(Type);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;

        return obj is Property other &&
               StringComparer.OrdinalIgnoreCase.Equals(other.Name, Name) 
               && Type == other.Type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type);
    }
}