using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A generic observer that wraps another object and adds some type and formatted UI friendly text for the value.
/// This observer also supportes inner/nested complex types like criterion or lists of objects.
/// </summary>
public class ValueObserver(object? value) : Observer<object?>(value)
{
    /// <summary>
    /// The text display for the value.
    /// This should either be the literal vlaue or some texttual representation of a complex value.
    /// Foir this we are using a custom extension to return the text based on the object type.
    /// </summary>
    public string Text => Model.ToText();

    /// <summary>
    /// The user-friendly type name of the value.
    /// </summary>
    public string Type => Model?.GetType().DisplayName() ?? "unknown";

    /// <summary>
    /// The <see cref="TypeGroup"/> to which this value belongs. 
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Model?.GetType());

    /// <summary>
    /// Indicates that the value is "empty" or has no value (null or empty text).
    /// </summary>
    public bool IsEmpty => Model is null || Model is string text && string.IsNullOrEmpty(text);

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Text.Satisfies(filter);
    }

    /// <inheritdoc />
    public override string ToString() => Model?.ToString() ?? string.Empty;
}