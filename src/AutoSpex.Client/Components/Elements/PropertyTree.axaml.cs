using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using JasperFx.Core;

namespace AutoSpex.Client.Components;

public class PropertyTree : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<PropertyTree, string?> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<PropertyTree, string?>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<PropertyTree, ElementObserver?> SourceElementProperty =
        AvaloniaProperty.RegisterDirect<PropertyTree, ElementObserver?>(
            nameof(SourceElement), o => o.SourceElement, (o, v) => o.SourceElement = v);

    #endregion

    private ElementObserver? _sourceElement;
    private string? _filterText;

    public ElementObserver? SourceElement
    {
        get => _sourceElement;
        set => SetAndRaise(SourceElementProperty, ref _sourceElement, value);
    }

    public string? FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    public ObservableCollection<PropertyObserver> Properties { get; } = [];

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (SourceElement is not null)
            HandleElementChange(SourceElement);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceElementProperty)
            HandleElementChange(change.GetNewValue<ElementObserver?>());

        if (change.Property == FilterTextProperty)
            UpdateProperties(change.GetNewValue<string?>());
    }

    private void HandleElementChange(ElementObserver? observer)
    {
        Properties.Clear();
        if (observer is null) return;
        Properties.AddRange(observer.Properties);
    }

    private void UpdateProperties(string? filter)
    {
        Properties.Clear();

        var properties = string.IsNullOrEmpty(filter)
            ? SourceElement?.Properties
            : SourceElement?.Properties.SelectMany(p => p.FindProperties(filter));

        if (properties is null) return;
        Properties.AddRange(properties );
    }
}