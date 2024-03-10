using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using JasperFx.Core;
using TextBox = Avalonia.Controls.TextBox;

namespace AutoSpex.Client.Components;

public class PropertyTree : TemplatedControl
{
    public static readonly DirectProperty<PropertyTree, ElementObserver?> SourceElementProperty =
        AvaloniaProperty.RegisterDirect<PropertyTree, ElementObserver?>(
            nameof(SourceElement), o => o.SourceElement, (o, v) => o.SourceElement = v);

    private readonly List<PropertyObserver> _properties = [];
    private ElementObserver? _sourceElement;
    private TextBox? _filterText;

    public ElementObserver? SourceElement
    {
        get => _sourceElement;
        set => SetAndRaise(SourceElementProperty, ref _sourceElement, value);
    }
    
    public ObservableCollection<PropertyObserver> Properties { get; } = [];

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceElementProperty)
            HandleElementChange(change.GetNewValue<ElementObserver>());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterText(e);

        if (SourceElement is not null)
            HandleElementChange(SourceElement);
    }

    private void RegisterFilterText(TemplateAppliedEventArgs args)
    {
        if (_filterText is not null) _filterText.TextChanged -= FilterTextChanged;
        _filterText = args.NameScope.Find<TextBox>("FilterText");
        if (_filterText is null) return;
        _filterText.TextChanged += FilterTextChanged;
    }

    private void FilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        UpdateProperties(textBox.Text);
    }

    private void HandleElementChange(ElementObserver? observer)
    {
        if (observer is null) return;

        _properties.Clear();
        _properties.AddRange(observer.Properties);
        
        UpdateProperties(_filterText?.Text);
    }
    
    private void UpdateProperties(string? filter)
    {
        Properties.Clear();
        var filtered = _properties.Where(p => FilterProperty(p, filter));
        Properties.AddRange(filtered);
    }

    private static bool FilterProperty(PropertyObserver property, string? text)
    {
        if (string.IsNullOrEmpty(text)) return true;

        if (property.Name.Contains(text)) return true;
        if (property.Type.Contains(text)) return true;
        if (property.Value?.ToString()?.Contains(text) is true) return true;
        
        return false;
    }
}