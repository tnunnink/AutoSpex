using System.Collections;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using L5Sharp.Core;

// ReSharper disable InvalidXmlDocComment

namespace AutoSpex.Engine;

public static class Extensions
{
    /// <summary>
    /// Gets a friendly type name for the provided type. I want this for the UI so that it is easier to read what the type
    /// is.
    /// </summary>
    /// <param name="type">The type to get the identifier for.</param>
    /// <returns>A string representing a UI friendly name of the type.</returns>
    public static string DisplayName(this Type? type)
    {
        if (type is null) return "unknown";
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
            return $"{string.Join(", ", type.GetGenericArguments().Select(DisplayName).ToArray())}[]";

        if (type.IsNullable())
            // ReSharper disable once TailRecursiveCall dont care
            return Nullable.GetUnderlyingType(type)!.DisplayName();

        if (type.IsGenericType)
            return type.Name.Split('`')[0] + "<" +
                   string.Join(", ", type.GetGenericArguments().Select(DisplayName).ToArray()) + ">";

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
            ICollection collection => $"[{collection.Count}]",
            IEnumerable enumerable =>
                $"{string.Join(", ", enumerable.GetType().GetGenericArguments().Select(DisplayName).ToArray())}s",
            _ => candidate?.ToString() ?? "null"
        };
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
    /// Gets the property value of the specified name from the JSON element.
    /// </summary>
    /// <param name="element">The JSON element to get the value from.</param>
    /// <param name="name">The name of the property to retrieve.</param>
    /// <returns>The JSON element of the specified name if it exists, null otherwise.</returns>
    public static JsonElement? Get(this JsonElement element, string name)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return element.TryGetProperty(name, out var value) ? value : null;
    }

    /// <summary>
    /// Determines whether the given type is enumerable.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>True if the type is enumerable, false otherwise.</returns>
    public static bool IsEnumerable(this Type type)
    {
        return type.IsAssignableTo(typeof(IEnumerable)) ||
               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    /// <summary>
    /// Determines if the specified type is nullable.
    /// </summary>
    /// <param name="type">The type to evaluate.</param>
    /// <returns>True if the type is nullable, false otherwise.</returns>
    private static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) is not null;
}