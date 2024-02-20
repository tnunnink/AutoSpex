using System;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AutoSpex.Client.Components;

[PseudoClasses(UnresolvedClass)]
public class PropertyEntry : TemplatedControl
{
    private const string UnresolvedClass = ":unresolved";

    #region Properties

    public static readonly DirectProperty<PropertyEntry, Type?> OriginTypeProperty =
        AvaloniaProperty.RegisterDirect<PropertyEntry, Type?>(
            nameof(OriginType), o => o.OriginType, (o, v) => o.OriginType = v, 
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<PropertyEntry, Property?> PropertyProperty =
        AvaloniaProperty.RegisterDirect<PropertyEntry, Property?>(
            nameof(Property), o => o.Property, (o, v) => o.Property = v, 
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<PropertyEntry, string?> PropertyNameProperty =
        AvaloniaProperty.RegisterDirect<PropertyEntry, string?>(
            nameof(PropertyName), o => o.PropertyName, (o, v) => o.PropertyName = v,
            defaultBindingMode: BindingMode.TwoWay);

    #endregion

    private Type? _originType;
    private Property? _property;
    private string? _propertyName;

    public Type? OriginType
    {
        get => _originType;
        set => SetAndRaise(OriginTypeProperty, ref _originType, value);
    }

    public string? PropertyName
    {
        get => _propertyName;
        set => SetAndRaise(PropertyNameProperty, ref _propertyName, value);
    }

    public Property? Property
    {
        get => _property;
        set => SetAndRaise(PropertyProperty, ref _property, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PropertyProperty)
            SetResolvedClass();
    }

    private void SetResolvedClass()
    {
        PseudoClasses.Remove(UnresolvedClass);

        if (string.IsNullOrEmpty(PropertyName))
            return;

        if (Property is null)
            PseudoClasses.Add(UnresolvedClass);
    }
}