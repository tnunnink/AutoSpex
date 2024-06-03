using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple object observer that wraps a given object value and adds some type and formatted UI friendly text for the
/// value.
/// </summary>
/// <param name="model"></param>
public class ValueObserver(object model) : Observer<object>(model)
{
    public string Text => Model.ToText();
    public string Type => Model.GetType().CommonName();
    public TypeGroup Group => TypeGroup.FromType(Model.GetType());

    public bool Passes(string? filter)
    {
        return string.IsNullOrEmpty(filter) || Text.PassesFilter(filter) || Type.PassesFilter(filter);
    }
}