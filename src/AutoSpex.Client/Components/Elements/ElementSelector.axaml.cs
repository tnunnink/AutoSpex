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
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

public class ElementSelector : TemplatedControl
{
    private const string PartFilterText = "FilterText";
    private const string PartElementList = "ElementList";

    #region Properties

    public static readonly StyledProperty<ControlTheme> ButtonThemeProperty =
        AvaloniaProperty.Register<ElementSelector, ControlTheme>(
            nameof(ButtonTheme));

    public static readonly StyledProperty<PlacementMode> FlyoutPlacementProperty =
        AvaloniaProperty.Register<ElementSelector, PlacementMode>(
            nameof(FlyoutPlacement), PlacementMode.BottomEdgeAlignedRight);

    public static readonly DirectProperty<ElementSelector, Element> ElementProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, Element>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v,
            defaultBindingMode: BindingMode.TwoWay,
            unsetValue: Element.Default);

    public static readonly DirectProperty<ElementSelector, bool> IsDefaultElementProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, bool>(
            nameof(IsDefaultElement), o => o.IsDefaultElement, (o, v) => o.IsDefaultElement = v);

    #endregion

    private Element _element = Element.Default;
    private bool _isDefaultElement;
    private TextBox? _textBox;
    private ListBox? _listBox;

    public Element Element
    {
        get => _element;
        set => SetAndRaise(ElementProperty, ref _element, value);
    }

    public bool IsDefaultElement
    {
        get => _isDefaultElement;
        set => SetAndRaise(IsDefaultElementProperty, ref _isDefaultElement, value);
    }

    public ControlTheme ButtonTheme
    {
        get => GetValue(ButtonThemeProperty);
        set => SetValue(ButtonThemeProperty, value);
    }

    public PlacementMode FlyoutPlacement
    {
        get => GetValue(FlyoutPlacementProperty);
        set => SetValue(FlyoutPlacementProperty, value);
    }

    public ObservableCollection<Element> Elements { get; } = new(Element.Selectable);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterText(e);
        RegisterElementList(e);
        IsDefaultElement = Element == Element.Default;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ElementProperty)
            IsDefaultElement = change.GetNewValue<Element>() == Element.Default;
    }


    private void RegisterFilterText(TemplateAppliedEventArgs args)
    {
        if (_textBox is not null) _textBox.TextChanged -= FilterTextChanged;
        _textBox = args.NameScope.Find<TextBox>(PartFilterText);
        if (_textBox is not null) _textBox.TextChanged += FilterTextChanged;
    }

    private void FilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        var filter = textBox.Text;

        var filtered = Element.Selectable
            .Where(x => x.Name.PassesFilter(filter))
            .OrderBy(x => x.Name);

        Elements.Refresh(filtered);
    }

    private void RegisterElementList(TemplateAppliedEventArgs args)
    {
        if (_listBox is not null) _listBox.PointerReleased -= ElementListPressed;
        _listBox = args.NameScope.Find<ListBox>(PartElementList);
        if (_listBox is not null) _listBox.PointerReleased += ElementListPressed;
    }

    private void ElementListPressed(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is not Control { DataContext: Element element } control) return;

        e.Handled = true;
        Element = element;

        var popup = control.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }
}