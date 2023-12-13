using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoSpex.Engine;

public static class Parser
{
    private static readonly Dictionary<Type, Func<string, object>> Registry = new();

    public static object Parse(Type type, string input)
    {
        return GetParser(type)(input) ?? throw new ArgumentException("");
    }

    public static T Parse<T>(string input) => (T) Parse(typeof(T), input);

    public static void Register<T>(Expression<Func<string, T>> parser)
    {
        // Compile the expression to a Func<string, T>
        var compiledParser = parser.Compile();

        // Cast the parsed value to object and add it to the registry
        Registry[typeof(T)] = input => compiledParser(input);
    }
    
    private static Func<string, object?> GetParser(Type type)
    {
        if (Registry.TryGetValue(type, out var parser))
            return parser;

        var converter = TypeDescriptor.GetConverter(type);

        if (converter.CanConvertFrom(typeof(string)))
            return s => converter.ConvertFrom(s);

        throw new InvalidOperationException($"No parse function has been defined for type '{type}'");
    }
    
    private static bool TryFindRegistered(Type type, out Func<string, object?>? parser)
    {
        parser = Registry.GetValueOrDefault(type);
        return parser is not null;
    }
    
    private static bool TryFindParserImplementation(Type type, out Func<string, object?> parser)
    {
        throw new NotImplementedException();
    }
    
    private static bool TryFindDescriptor(Type type, out Func<string, object?> parser)
    {
        throw new NotImplementedException();
    }

    private static bool TryFindFactory(Type type, out Func<string, object?> parser)
    {
        throw new NotImplementedException();
    }
}

public interface IParser<out T>
{
    T Parse(string input);
}