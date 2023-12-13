using System.Reflection;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Property
{
    public Property(string name, Type type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name can not be null or empty to initialize a property.");

        Name = name;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public Property(PropertyInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
        Name = info.Name;
        Type = info.PropertyType;
    }

    public string Name { get; }

    public Type Type { get; }

    public IEnumerable<Property> Properties => GetProperties(Type);

    public override bool Equals(object? obj) => obj is Property other && other.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => Name;

    private IEnumerable<Property> GetProperties(Type type)
    {
        if (typeof(LogixElement).IsAssignableFrom(type))
        {
            var element = Element.FromName(Type.Name);
            return element.Properties;
        }

        if (type.Assembly.Equals(typeof(LogixElement).Assembly))
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => new Property(p));
        }

        return Enumerable.Empty<Property>();
    }
}