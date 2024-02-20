using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using DynamicData;
using DynamicData.Binding;

namespace AutoSpex.Client.Components;

public class ElementFlyout : TemplatedControl
{
    private const string PartTextBox = "PART_TextBox";
    
    #region Properties

    public static readonly DirectProperty<ElementFlyout, Element?> ElementProperty =
        AvaloniaProperty.RegisterDirect<ElementFlyout, Element?>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<ElementFlyout, bool> ShowAllProperty =
        AvaloniaProperty.RegisterDirect<ElementFlyout, bool>(
            nameof(ShowAll), o => o.ShowAll, (o, v) => o.ShowAll = v, defaultBindingMode: BindingMode.TwoWay);

    #endregion

    private TextBox? _textBox;
    private Element? _element;
    private readonly SourceList<Element> _source = new();
    private readonly ReadOnlyObservableCollection<Element> _elements;
    private bool _showAll;

    public ElementFlyout()
    {
        _source.Connect()
            .Sort(SortExpressionComparer<Element>.Ascending(t => t.Name))
            .Bind(out _elements)
            .Subscribe();

        _source.AddRange(Element.Components);
    }

    static ElementFlyout()
    {
        PointerReleasedEvent.AddClassHandler<ElementFlyout>((e, a) => e.PointerReleasedHandle(a));
    }

    public IEnumerable<Element> Elements => _elements;

    public Element? Element
    {
        get => _element;
        set => SetAndRaise(ElementProperty, ref _element, value);
    }

    public bool ShowAll
    {
        get => _showAll;
        set => SetAndRaise(ShowAllProperty, ref _showAll, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterText(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowAllProperty)
            OnShowAllChanged(change);
    }


    private void RegisterFilterText(TemplateAppliedEventArgs args)
    {
        if (_textBox is not null) _textBox.TextChanged -= FilterTextChanged;
        _textBox = args.NameScope.Find<TextBox>(PartTextBox);
        if (_textBox is not null) _textBox.TextChanged += FilterTextChanged;
    }

    private void FilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;

        var filter = textBox.Text;

        _source.Clear();

        if (!string.IsNullOrEmpty(filter))
        {
            var filtered = Element.List.Where(x => x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase));
            _source.AddRange(filtered);
            return;
        }

        _source.AddRange(Element.Components);
        ShowAll = false;
    }

    private void OnShowAllChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var showAll = args.GetNewValue<bool>();

        _source.Clear();

        if (showAll)
        {
            _source.AddRange(Element.Selectable);
            return;
        }

        _source.AddRange(Element.Components);
    }

    private void PointerReleasedHandle(RoutedEventArgs args)
    {
        if (args.Source is not Control {DataContext: Element element}) return;

        args.Handled = true;
        Element = element;

        var popup = this.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }
}