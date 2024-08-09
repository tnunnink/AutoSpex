using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple object observer that wraps a object value and adds some type and formatted UI friendly text for the
/// value.
/// </summary>
/// <param name="value">The object value to wrap.</param>
public class ValueObserver(object? value) : NullableObserver<object>(value)
{
    /// <summary>
    /// The value of the observer instance. 
    /// </summary>
    /// <remarks>
    /// If the underlying value is a model that has a corresponding observer type, then this value returns a wrapped
    /// observer instance of those types. Any other type is the direct type value.
    /// </remarks>
    public object? Value => GetValue();

    /// <summary>
    /// The text display for the value. This should be a common name that can be used to identify the value.
    /// </summary>
    public string Text => Model.ToText();

    /// <summary>
    /// The user-friendly type name of the value.
    /// </summary>
    public string Type => Model?.GetType().CommonName() ?? string.Empty;

    /// <summary>
    /// The <see cref="TypeGroup"/> in which this value belongs. 
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Model?.GetType());

    /// <summary>
    /// Indicates that the value is empty (null or empty text).
    /// </summary>
    public bool IsEmpty => Value is null || (Value is string text && string.IsNullOrEmpty(text));

    /// <summary>
    /// A default or null value observer instance.
    /// </summary>
    public static ValueObserver Default => new(default);

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter)
               || Text.Satisfies(filter)
               || Type.Satisfies(filter)
               || Group.Name.Satisfies(filter);
    }

    /// <inheritdoc />
    public override string ToString() => Model?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns the wrapped model value if the model has a corresponding observer type.
    /// </summary>
    private object? GetValue()
    {
        return Model switch
        {
            Variable variable => new VariableObserver(variable),
            Reference reference => new ReferenceObserver(reference),
            Criterion criterion => new CriterionObserver(criterion),
            List<Argument> arguments => arguments.Select(a => new ArgumentObserver(a)),
            _ => Model
        };
    }
}