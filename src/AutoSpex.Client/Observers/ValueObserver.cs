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
    public bool IsEmpty => Model is null || (Model is string text && string.IsNullOrEmpty(text));

    /// <summary>
    /// A default or null value observer instance.
    /// </summary>
    public static ValueObserver Default => new(default);

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Text.Satisfies(filter);
    }

    /// <inheritdoc />
    public override string ToString() => Model?.ToString() ?? string.Empty;
}