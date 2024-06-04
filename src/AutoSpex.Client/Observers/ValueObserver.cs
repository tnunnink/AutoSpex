using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple object observer that wraps a object value and adds some type and formatted UI friendly text for the
/// value.
/// </summary>
/// <param name="model">The object value to wrap.</param>
public class ValueObserver(object model) : Observer<object>(model)
{
    /// <summary>
    /// The text display for the value. This should be a common name that can be used to identify the value.
    /// </summary>
    public string Text => Model.ToText();

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
        return string.IsNullOrEmpty(filter) || Text.PassesFilter(filter) || Type.PassesFilter(filter);
    }

    private string GetTypeName()
    {
        var type = Model is VariableObserver variable ? variable.Model.GetType() : Model.GetType();
        return type.CommonName();
    }

    private TypeGroup GetTypeGroup()
    {
        var type = Model is VariableObserver variable ? variable.Model.GetType() : Model.GetType();
        return TypeGroup.FromType(type);
    }
}