using System.IO.Compression;
using System.Reflection;
using System.Text;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public static class Extensions
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
        if (type == typeof(bool)) return "bool";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(sbyte)) return "sbyte";
        if (type == typeof(short)) return "short";
        if (type == typeof(ushort)) return "ushort";
        if (type == typeof(int)) return "int";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(long)) return "long";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(string)) return "string";

        if (type.IsGenericType)
            return type.Name.Split('`')[0] + "<" +
                   string.Join(", ", type.GetGenericArguments().Select(TypeIdentifier).ToArray()) + ">";

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return underlying.TypeIdentifier();

        return type.Name;
    }

    public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) is not null;

    public static IEnumerable<object> GetOptions(this Type type)
    {
        var group = TypeGroup.FromType(type);

        if (group == TypeGroup.Boolean)
            return new object[] {true, false};

        if (type.IsEnum)
            return Enum.GetNames(type);

        if (typeof(LogixEnum).IsAssignableFrom(type))
            return LogixEnum.Options(type);

        return Enumerable.Empty<object>();
    }

    public static Type? ToType(this string? typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return default;

        var nativeType = Type.GetType(typeName);
        if (nativeType is not null) return nativeType;

        var logixType = typeof(LogixElement).Assembly.GetType(typeName);
        if (logixType is not null) return logixType;

        var engineType = typeof(Extensions).Assembly.GetType(typeName);
        return engineType;
    }

    public static string Compress(this string data)
    {
        var bytes = Encoding.Unicode.GetBytes(data);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }

        return Convert.ToBase64String(mso.ToArray());
    }

    public static string Decompress(this string data)
    {
        var bytes = Convert.FromBase64String(data);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }

        return Encoding.Unicode.GetString(mso.ToArray());
    }
}