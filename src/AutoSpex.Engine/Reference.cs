using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// A type that allows us to dynamically specify how to retreive a value at runtime so that we don't have to statically
/// configure a criterion argument.
/// </summary>
public class Reference()
{
    public const char KeyStart = '{';
    public const char KeyEnd = '}';
    public const char PropertySeprarotr = '.';
    public const char SpecialStart = '$';
    public const char VariableStart = '@';
    
    private readonly Func<object?, object?>? _resolver;
    
    private Reference(string key, Func<object?, object?> resolver) : this()
    {
        Key = key;
        _resolver = resolver;
    }

    public Reference(string key) : this()
    {
        Key = key;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string? Property { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public object? Value { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public static Reference This => new Reference("$this", x => x);
    
    /// <summary>
    /// 
    /// </summary>
    public static Reference Required => new Reference("$required", x => throw new InvalidOperationException(""));

    /// <summary>
    /// Given an object, try to resolve the value based on this configured reference. This assumes the provided object
    /// is a LogixElement from which we can obtain the root source L5X.
    /// As of now this is always the case. If it ends up not being the case, we will have to resolve references ahead
    /// of time so that we can return the correct data.
    /// </summary>
    /// <param name="candidate">The candidate object from which to resolve the configured reference.</param>
    /// <returns>The object that represents the referenced value.</returns>
    public object? Resolve(object? candidate)
    {
        //For special built-in references use the specified resolver function.
        //These reference should not rely on external data (other than perhaps the input object).  
        if (_resolver is not null)
        {
            _resolver.Invoke(candidate);
        }
        
        return Value;
        /*if (candidate is not LogixElement element || element.L5X is null) return null;

        //We want to use Get because if the scope is invalid we will get and exception and report that to the user.
        var scoped = element.L5X.Get(Scope);

        if (string.IsNullOrEmpty(Property)) return scoped;

        //If configured return the sub property value of the object.
        var property = Element.This.GetProperty(Property);
        return property.GetValue(scoped);*/
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return string.Concat(KeyStart, Key, KeyEnd, PropertySeprarotr, Property).TrimEnd(PropertySeprarotr);
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