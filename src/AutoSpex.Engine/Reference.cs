using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// A type that allows us to dynamically specify how to retreive a value at runtime so that we don't have to statically
/// configure a criterion argument.
/// </summary>
public class Reference
{
    //Special characters.
    public const char KeyStart = '{';
    public const char KeyEnd = '}';
    public const char PropertySeparator = '.';
    public const char PathSeparator = '/';
    public const char SpecialStart = '$';
    public const char VariableStart = '@';

    /// <summary>
    /// The collection of preconfigured special reference objects that we can resolve data for statically
    /// (without use of external resources).
    /// </summary>
    private static readonly Dictionary<string, Func<object?, object?>> Resolvers = 
        new Dictionary<string, Func<object?, object?>>()
    {
        {"$this", x => x},
        {"$required", _ => throw new InvalidOperationException("Value is required for evaluation. Confiugre an override to replace required reference(s).")},
    };
    
    /// <summary>
    /// The function that returns the value this reference should resolve to.
    /// For special reference this is predefined.
    /// For source and variables references this will need to be set externally as it will require database retrieval
    /// </summary>
    private Func<object?, object?>? _resolver;

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
    /// 1. A source reference which uses the scope path syntax and identifies a reference to a source element to retrieve.
    /// 2. A special reference which is a built in reference that we can resolve the value for statically or without external resources.
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
    /// The scope that defines the path to a element to retrieve from an L5X source file.
    /// This will be empty for non source type references.
    /// </summary>
    public Scope Scope => Key.Contains(PathSeparator) ? Scope.To(Key) : Scope.Empty;

    /// <summary>
    /// Indicates whether this reference represents a source reference (contains '/').
    /// </summary>
    public bool IsSource => Key.Contains(PathSeparator);

    /// <summary>
    /// Indicates whenther this reference represents a special reference (starts with '$'). 
    /// </summary>
    public bool IsSpecial => Key.StartsWith(SpecialStart);

    /// <summary>
    /// Indicates whether this reference represents a variable reference (starts with '@').
    /// </summary>
    public bool IsVariable => Key.StartsWith(VariableStart);

    /// <summary>
    /// Gets the collection of special predefined references that have static resolver funtions (e.g. $this, $required, etc.).
    /// </summary>
    public static IEnumerable<Reference> Special => Resolvers.Select(r => new Reference(r.Key));
    
    /// <summary>
    /// Creates a new special self reference that will always resolve to the object provided bo the resolve function.
    /// </summary>
    public static Reference This => new Reference("$this");
    
    /// <summary>
    /// Creates a new special required <see cref="Reference"/> that will always throw an exception, causing a spec error.
    /// This special reference requires the user to replace with a know or expected value.
    /// This is useful for when we need to force the user to supply an argument to pass the spec.
    /// </summary>
    public static Reference Required => new Reference("$required");

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

    /// <summary>
    /// Configures this reference to use the provided resolver function to obtain the value needed when
    /// the <see cref="Resolve"/> function is invoked.
    /// </summary>
    /// <param name="resolver">The function that resolves the value based on the configured reference.</param>
    /// <exception cref="InvalidOperationException">Throw when this reference is a special preconfigured reference.</exception>
    public void ResolveUsing(Func<object?, object?> resolver)
    {
        if (IsSpecial)
            throw new InvalidOperationException("Special references are predefined and can not resolve to external data.");
        
        _resolver = resolver;
    }

    /// <summary>
    /// Extracts the value this reference refers to from the provided source and sets the internal resolver funtion
    /// to return that value so that when <see cref="Resolve"/> is called the correct object is returned.
    /// </summary>
    /// <param name="content">The source <see cref="L5X"/> needed to resolve this reference.</param>
    /// <exception cref="InvalidOperationException">This is not a <see cref="IsSource"/> reference.</exception>
    public void ResolveUsing(L5X? content)
    {
        if (!IsSource)
            throw new InvalidOperationException("Can only resolve source type references to an L5X.");

        if (content is null)
        {
            _resolver = _ => throw new InvalidOperationException($"No source with name '{Scope.Controller}' was found.");
            return;
        }
        
        if (!content.TryGet(Scope.LocalPath, out var element))
        {
            _resolver = _ => throw new InvalidOperationException($"No element with scope '{Scope.LocalPath}' was found.");
            return;
        }

        //Cloning will ensure the instance doesn't hold onto the root L5X which will comsume more memory.
        //We just want the object/value defined by this reference.
        //If we encounter an exception then the resolver will re-throw it.
        
        if (element is null || string.IsNullOrEmpty(Property))
        {
            var instance = element?.Clone();
            _resolver = _ => instance;
            return;
        }
        
        try
        {
            var property = Engine.Property.This(element.GetType()).GetProperty(Property);
            var value = property.GetValue(element);
            value = value is LogixElement nested ? nested.Clone() : value;
            _resolver = _ => value;
        }
        catch (Exception e)
        {
            _resolver = _ => throw e;
        }
    }

    /// <summary>
    /// Given an object, try to resolve the value based on this configured reference.
    /// </summary>
    /// <param name="candidate">The candidate object from which to resolve the configured reference.</param>
    /// <returns>The object that represents the referenced value.</returns>
    /// <exception cref="InvalidOperationException">Throw when the reference resolver has not been configured..</exception>
    public object? Resolve(object? candidate)
    {
        //Special references will be handled separately since we want to then use the property extension.
        if (IsSpecial && Resolvers.TryGetValue(Key, out var resolver))
        {
            var value = resolver.Invoke(candidate);
            if (value is null || string.IsNullOrEmpty(Property)) return value;
            var property = Engine.Property.This(value.GetType()).GetProperty(Property);
            return property.GetValue(value);
        }
        
        //Any other unknown reference is subject to being resolved externally.
        if (_resolver is null)
            throw new InvalidOperationException($"Could not resolve reference {Key} to a runtime value.");
        
        //External references will get this reference instance instead of the candidate object.
        return _resolver.Invoke(this);
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