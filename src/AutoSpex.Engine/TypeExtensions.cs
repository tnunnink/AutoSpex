using System.Reflection;

namespace AutoSpex.Engine;

public static class TypeExtensions
{
    public static IEnumerable<Property> Properties(this Type type, Property? parent = default)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            var origin = parent is not null ? parent.Origin : type;
            var path = parent is not null ? $"{parent.Path}.{property.Name}" : property.Name;
            var propertyType = property.PropertyType;
            yield return new Property(origin, path, propertyType);
        }
    }

    public static Property? Property(this Type type, string? path)
    {
        if (path is null) return default;

        var element = Element.List.SingleOrDefault(e => e.Type == type);

        Property? property = null;
        var properties = element is not null ? element.Properties.ToList() : type.Properties().ToList();

        while (!string.IsNullOrEmpty(path) && properties.Count > 0)
        {
            var dot = path.IndexOf('.');
            var member = dot >= 0 ? path[..dot] : path;
            property = properties.SingleOrDefault(p => p.Name == member);
            properties = property?.Properties.ToList() ?? Enumerable.Empty<Property>().ToList();
            path = dot >= 0 ? path[(dot + 1)..] : string.Empty;
        }

        return property;
    }

    public static string TypeIdentifier(this Type type)
    {
        if (type == typeof(int))
            return "int";
        if (type == typeof(short))
            return "short";
        if (type == typeof(byte))
            return "byte";
        if (type == typeof(bool))
            return "bool";
        if (type == typeof(long))
            return "long";
        if (type == typeof(float))
            return "float";
        if (type == typeof(double))
            return "double";
        if (type == typeof(decimal))
            return "decimal";
        if (type == typeof(string))
            return "string";

        if (type.IsGenericType)
            return type.Name.Split('`')[0] + "<" +
                   string.Join(", ", type.GetGenericArguments().Select(TypeIdentifier).ToArray()) + ">";

        return type.Name;
    }
}