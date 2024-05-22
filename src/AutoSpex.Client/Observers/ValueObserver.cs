using System;
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
    /// <summary>
    /// 
    /// </summary>
    public string Text => Model.ToText();
    
    public Type Type => Model.GetType();
    public string TypeName => Model.GetType().CommonName();
    public TypeGroup Group => TypeGroup.FromType(Type);
}