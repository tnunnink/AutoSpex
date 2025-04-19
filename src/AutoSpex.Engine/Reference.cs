using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// A type that allows us to dynamically specify how to retrieve a value at runtime so that we don't have to statically
/// configure a criterion argument.
/// </summary>
public class Reference
{
    //Special characters.
    public const string ThisKey = "$this";
    public const char KeyStart = '{';
    public const char KeyEnd = '}';
    public const char PropertySeparator = '.';
    public const char SpecialStart = '$';

    /// <summary>
    /// The function that returns the value this reference should resolve to.
    /// For special reference this is predefined.
    /// For source and variables references this will need to be set externally as it will require database retrieval
    /// </summary>
    private Func<object?, object?>? _resolver;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="resolver"></param>
    /// <exception cref="ArgumentException"></exception>
    private Reference(string key, Func<object?, object?> resolver)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Reference requires a Key to be created.");

        Key = key;
        _resolver = resolver;
    }

    /// <summary>
    /// Creates a new <see cref="Reference"/> having the provided key and optional property extension.
    /// </summary>
    /// <param name="key">The string key that identifies the reference. This can be a source reference or variable reference.</param>
    /// <param name="property">The optional property name extension that we can retrieve from the resolved object reference.</param>
    public Reference(string key, string? property = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Reference requires a Key to be created.");
        
        Key = key;
        Property = property;
    }

    /// <summary>
    /// The text value that represents an object this reference should resolve to.
    /// </summary>
    /// <remarks>
    /// This can be one of three different types:
    /// 1. A special built-in reference that we can resolve the value for statically os in some special way.
    /// 2. A source reference which uses the scope path syntax and identifies a reference to a source element to retrieve.
    /// 3. A variable reference which is configured by in the application and can represent some reusable dynamic data to point to.
    /// </remarks>
    public string Key { get; private init; }

    /// <summary>
    /// The property name extension that represents the value of the resolved object to retrieve.
    /// </summary>
    /// <remarks>
    /// This is optional, but allows us to further drill down into an object and get some more specific value.
    /// This works based off the type returned when we invoke the reference resolver.
    /// </remarks>
    public string? Property { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsThis => Key == ThisKey; 
    
    /// <summary>
    /// Returns the special self-reference that will always resolve to the object provided to the resolve function.
    /// </summary>
    public static Reference This => new Reference(ThisKey, x => x);

    /// <summary>
    /// Parses the input text to create a new instance of <see cref="Reference"/>.
    /// </summary>
    /// <param name="text">The text containing the reference key and optional property extension.</param>
    /// <returns>A new <see cref="Reference"/> object parsed from the input text.</returns>
    public static Reference Parse(string? text)
    {
        if (string.IsNullOrEmpty(text))
            throw new FormatException("Can not parse Reference from a null or empty string.");

        if (!text.StartsWith(KeyStart) && !text.Contains(KeyEnd))
            throw new FormatException("Reference string should start with '{' and contain '}'.");

        var key = text[..text.IndexOf(KeyEnd)].TrimStart(KeyStart);
        var property = text[(text.IndexOf(KeyEnd) + 1)..].TrimStart(PropertySeparator);

        return new Reference(key, property);
    }

    /// <summary>
    /// Checks if the provided text starts with the KeyStart character and contains the KeyEnd character.
    /// </summary>
    /// <param name="text">The text to be validated.</param>
    /// <returns>True if the text starts with the KeyStart character and contains the KeyEnd character, otherwise false.</returns>
    public static bool IsValid(string text)
    {
        return text.StartsWith(Reference.KeyStart) && text.Contains(Reference.KeyEnd);
    }
    
    
    public void ResolveTo(Variable variable)
    {
        //Only update if the reference is not a special reference
        if (Key == "$this") 
            return;

        //Just points to the variable value.
        _resolver = _ => variable.Value;
    }

    /// <summary>
    /// Given an object, try to resolve the value based on this configured reference.
    /// </summary>
    /// <param name="candidate">The candidate object from which to resolve the configured reference.</param>
    /// <returns>The object that represents the referenced value.</returns>
    /// <exception cref="InvalidOperationException">Throw when the reference resolver has not been configured.</exception>
    public object? Resolve(object? candidate)
    {
        if (_resolver is null)
            throw new InvalidOperationException($"Could not resolve reference {Key} to a runtime value.");
        
        var resolved = _resolver?.Invoke(candidate);
        
        if (resolved is null || string.IsNullOrEmpty(Property)) 
            return resolved;
        
        var property = Engine.Property.This(resolved).GetProperty(Property);
        return property.GetValue(resolved);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Concat(KeyStart, Key, KeyEnd, PropertySeparator, Property).TrimEnd(PropertySeparator);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;

        return obj switch
        {
            Reference other => Key == other.Key && Property == other.Property, 
            _ => false
        };
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Property);
    }
}