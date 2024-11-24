using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// A type that can allow us to dynamically specify/retreive a value from a different part of the source file so that
/// we don't have to statically configure an argument.
/// </summary>
public class Reference
{
    public const char StartKey = '{';
    public const char EndKey = '}';

    public Guid SourceId { get; set; } = Guid.Empty;
    
    /// <summary>
    /// The scope of the logix object to obtain. 
    /// </summary>
    public Scope Scope { get; set; } = Scope.Empty;

    /// <summary>
    /// The element type of the reference. This is parsed from the scope type for which we have a matching element
    /// that gives us necessary type information.
    /// </summary>
    public Element Element => Scope.Type != ScopeType.Null ? Element.FromName(Scope.Type) : Element.Default;

    /// <summary>
    /// The optional property path that specifies a sub value of the returned object instance.
    /// </summary>
    public string Property { get; private set; } = string.Empty;

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
        if (candidate is not LogixElement element || element.L5X is null) return null;
        
        //We want to use Get because if the scope is invalid we will get and exception and report that to the user.
        var scoped = element.L5X.Get(Scope);

        if (string.IsNullOrEmpty(Property)) return scoped;
        
        //If configured return the sub property value of the object.
        var property = Element.This.GetProperty(Property);
        return property.GetValue(scoped);
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return string.Concat(StartKey, Scope, EndKey, '.', Property);
    }
}