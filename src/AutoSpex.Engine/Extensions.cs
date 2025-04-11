using System.Collections;
using System.Dynamic;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

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

        if (type == typeof(ExpandoObject)) return "object";

        //Collections
        if (type.IsArray) return $"{type.GetElementType().DisplayName()}[]";

        if (type.IsEnumerable())
        {
            var args = type.GetGenericArguments().Select(DisplayName).ToArray();
            return $"{string.Join(",", args)}[]";
        }

        // ReSharper disable once TailRecursiveCall
        if (type.IsNullable()) return Nullable.GetUnderlyingType(type)!.DisplayName();

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
            KeyValuePair<string, object> pair => pair.Key,
            string text => text,
            IEnumerable enumerable => $"[{string.Join(',', enumerable.Cast<object>().Select(x => x.ToText()))}]",
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
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <param name="token"></param>
    public static async Task CompressToAsync(this string data, string fileName, CancellationToken token = default)
    {
        var bytes = Encoding.UTF8.GetBytes(data);

        using var inputStream = new MemoryStream(bytes);
        await using var outputStream = File.Create(fileName);
        await using var zipStream = new GZipStream(outputStream, CompressionMode.Compress);
        await inputStream.CopyToAsync(zipStream, token);
    }

    /// <summary>
    /// Decompresses the specified file and returns the decompressed data as a base64-encoded string.
    /// </summary>
    /// <param name="fileName">The file name of the compressed file to decompress.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the decompressed data as a base64-encoded string.</returns>
    public static async Task<string> DecompressToAsync(this string fileName, CancellationToken token = default)
    {
        await using var inputStream = File.OpenRead(fileName);
        await using var zipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        await zipStream.CopyToAsync(outputStream, token);

        return Encoding.UTF8.GetString(outputStream.ToArray());
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
    /// Calculates the percentage of elements in the source that satisfy the given predicate.
    /// </summary>
    /// <param name="source">The IEnumerable of elements to calculate the percentage from.</param>
    /// <param name="predicate">The predicate to apply to each element.</param>
    /// <typeparam name="T">The type of elements in the source.</typeparam>
    /// <returns>The percentage of elements that satisfy the predicate as a double value.</returns>
    public static double Percent<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var total = 0;
        var count = 0;

        foreach (var item in source)
        {
            ++count;
            if (predicate(item))
            {
                total += 1;
            }
        }

        return count > 0 ? 100.0 * total / count : 0;
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

    /// <summary>
    /// Executes the task without waiting for its completion, and optionally handles exceptions that may occur.
    /// </summary>
    /// <param name="task">The task to be executed.</param>
    /// <param name="errorHandler">An optional action to handle exceptions thrown by the task.</param>
    public static void FireAndForget(this Task task, Action<Exception>? errorHandler = null)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted && errorHandler != null)
                errorHandler(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}