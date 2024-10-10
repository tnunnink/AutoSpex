using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using L5Sharp.Core;

// ReSharper disable InvalidXmlDocComment

namespace AutoSpex.Engine;

public static class Extensions
{
    /// <summary>
    /// Returns the current type or the inner generic argument type if the type is a generic type.
    /// </summary>
    /// <param name="type">The type ro evaluate.</param>
    /// <returns>A type representing this type if not generic, or the inner generic argument type if it is.</returns>
    /// <remarks>
    /// This is so that collections or other generics we can get the inner type to pass down to nested
    /// criterion objects, so they know which properties to resolve.
    /// </remarks>
    public static Type SelfOrInnerType(this Type type)
    {
        if (!type.IsGenericType) return type;
        var arguments = type.GetGenericArguments();
        return arguments[0];
    }

    /// <summary>
    /// Gets a friendly type name for the provided type. I want this for the UI so it is easier to read what the type
    /// is.
    /// </summary>
    /// <param name="type">The type to get the identifier for.</param>
    /// <returns>A string representing a UI friendly name of the type.</returns>
    public static string CommonName(this Type type)
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

        if (type.IsEnumerable())
            return $"{string.Join(", ", type.GetGenericArguments().Select(CommonName).ToArray())}[]";

        if (type.IsNullable())
            // ReSharper disable once TailRecursiveCall dont care
            return Nullable.GetUnderlyingType(type)!.CommonName();

        if (type.IsGenericType)
            return type.Name.Split('`')[0] + "<" +
                   string.Join(", ", type.GetGenericArguments().Select(CommonName).ToArray()) + ">";

        return type.Name;
    }

    /// <summary>
    /// Based on the object type return a UI friendly text representation for which we can identify this object value.
    /// </summary>
    public static string ToText(this object? candidate)
    {
        return candidate switch
        {
            bool b => b.ToString().ToLowerInvariant(),
            LogixEnum enumeration => enumeration.Name,
            LogixScoped scoped => scoped.Scope,
            string text => text, // this needs to be before IEnumerable since string is enumerable
            IEnumerable enumerable =>
                $"{string.Join(", ", enumerable.GetType().GetGenericArguments().Select(CommonName).ToArray())}s",
            _ => candidate?.ToString() ?? "null"
        };
    }

    /// <summary>
    /// Given a type returns a collection of possible values. This is meant primarily for enumeration types so that we
    /// can provide the user with a selectable set of options for a given enum value. This however will also return
    /// true/false for boolean type and empty collection for anything else (numbers, string, collections, complex objects).
    /// </summary>
    /// <param name="type">The type for which to determine the options.</param>
    /// <returns>A collection of typed value options for the specified type if found. Otherwise, and empty collection.</returns>
    public static IEnumerable<object> GetOptions(this Type type)
    {
        var group = TypeGroup.FromType(type);

        if (group == TypeGroup.Boolean)
            return new object[] { true, false };

        if (type.IsEnum)
            return Enum.GetNames(type);

        return typeof(LogixEnum).IsAssignableFrom(type) ? LogixEnum.Options(type) : Enumerable.Empty<object>();
    }

    /// <summary>
    /// Returns the <see cref="Type"/> object from a given type name.
    /// </summary>
    /// <param name="typeName">The name of the type.</param>
    /// <returns>A <see cref="Type"/> object if found, otherwise null.</returns>
    /// <remarks>
    /// This is a specialized extension which tries to get the Type form a string which is how some data
    /// will be persisted into the database. This method should work for standard .NET types, types defined in L5Sharp,
    /// as well as types defined in this AutoSpex.Engine project.
    /// </remarks>
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

    /// <summary>
    /// Compresses a string data value using built in .net <see cref="GZipStream"/> class.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>A base 64 string representing the compressed data of the clear text provided.</returns>
    /// <remarks>
    /// I'm using this for L5X files since we store them in the project database, I want to conserve as much
    /// physical memory as possible as sources are added.
    /// </remarks>
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

    /// <summary>
    /// Decompresses a string data value using built in .net <see cref="GZipStream"/> class.
    /// </summary>
    /// <param name="data">The data to decompress.</param>
    /// <returns>A string representation of the decompressed data.</returns>
    /// <remarks>
    /// I'm using this for L5X files since we store them in the project database, I want to conserve as much
    /// physical memory as possible as sources are added.
    /// </remarks>
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

    /// <summary>
    /// Determines whether the input string satisfies the given filter text.
    /// </summary>
    /// <param name="input">The input string to evaluate.</param>
    /// <param name="text">The filter text to check against the input string.</param>
    /// <returns>True if the input string satisfies the filter text, false otherwise.</returns>
    public static bool Satisfies(this string? input, string? text)
    {
        if (input is null) return false;
        return string.IsNullOrEmpty(text) || input.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the given type is enumerable.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>True if the type is enumerable, false otherwise.</returns>
    private static bool IsEnumerable(this Type type)
    {
        return type.IsAssignableTo(typeof(IEnumerable)) || type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    /// <summary>
    /// Determines if the specified type is nullable.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>True if the type is nullable, false otherwise.</returns>
    private static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) is not null;
}