using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple object observer that wraps a object value and adds some type and formatted UI friendly text for the
/// value.
/// </summary>
/// <param name="value">The object value to wrap.</param>
public class ValueObserver(object? value) : Observer
{
    /// <summary>
    /// 
    /// </summary>
    public object? Value { get; } = value;

    /// <summary>
    /// The text display for the value. This should be a common name that can be used to identify the value.
    /// </summary>
    public string Text => Value.ToText();

    /// <summary>
    /// The user-friendly type name of the value.
    /// </summary>
    public string Type => GetTypeName();

    /// <summary>
    /// The <see cref="TypeGroup"/> in which this value belongs. 
    /// </summary>
    public TypeGroup Group => GetTypeGroup();

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter)
               || Text.Satisfies(filter)
               || Type.Satisfies(filter)
               || Group.Name.Satisfies(filter);
    }

    private string GetTypeName()
    {
        var type = Value is VariableObserver variable ? variable.Model.GetType() : Value?.GetType();
        return type?.CommonName() ?? string.Empty;
    }

    private TypeGroup GetTypeGroup()
    {
        var type = Value is VariableObserver variable ? variable.Model.GetType() : Value?.GetType();
        return TypeGroup.FromType(type);
    }
}