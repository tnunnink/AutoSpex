using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AutoSpex.Client.Components;

public class ElementList : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<ElementList, Element> ElementProperty =
        AvaloniaProperty.RegisterDirect<ElementList, Element>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v,
            defaultBindingMode: BindingMode.TwoWay,
            unsetValue: Element.Default);

    public static readonly DirectProperty<ElementList, string?> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<ElementList, string?>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v);

    #endregion

    private Element _element = Element.Default;
    private string? _filterText;

    public Element Element
    {
        get => _element;
        set => SetAndRaise(ElementProperty, ref _element, value);
    }

    public string? FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateElements();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FilterTextProperty)
            UpdateElements(change.GetNewValue<string?>());
    }

    public ObservableCollection<Element> Elements { get; } = new(Element.Selectable);

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e is not
            {
                Source: Control { DataContext: Element element } control, InitialPressMouseButton: MouseButton.Left
            }) return;

        e.Handled = true;
        Element = element;
        var popup = control.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }

    private void UpdateElements(string? filter = default)
    {
        var elements = Element.Selectable.Where(e => FilterElement(e, filter)).OrderBy(x => x.Name);
        Elements.Refresh(elements);
    }

    private static bool FilterElement(Element element, string? filter)
    {
        return string.IsNullOrEmpty(filter) || element.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}