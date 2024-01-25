using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using DynamicData;
using DynamicData.Binding;

namespace AutoSpex.Client.Components;

public class ElementSelector : ContentControl
{
    private Element? _selectedElement;
    private readonly SourceList<Element> _source = new();
    private readonly ReadOnlyObservableCollection<Element> _elements;
    private string _filterText = string.Empty;
    private bool _showAll;
    
    public ElementSelector()
    {
        _source.Connect()
            .Sort(SortExpressionComparer<Element>.Ascending(t => t.Name))
            .Bind(out _elements)
            .Subscribe();
        
        _source.AddRange(Element.List.Where(e => e.IsComponent));
    }

    static ElementSelector()
    {
        FilterTextProperty.Changed.AddClassHandler<ElementSelector>((e, a) => e.OnFilterTextChanged(a));
        ShowAllProperty.Changed.AddClassHandler<ElementSelector>((e, a) => e.OnShowAllChanged(a));
        SelectedElementProperty.Changed.AddClassHandler<ElementSelector>((e, a) => e.OnSelectedElementChanged(a));
    }

    #region AvaloniaProperties

    public static readonly DirectProperty<ElementSelector, IEnumerable<Element>> ElementsProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, IEnumerable<Element>>(nameof(Elements), o => o.Elements);
    
    public static readonly DirectProperty<ElementSelector, Element?> SelectedElementProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, Element?>(
            nameof(SelectedElement), o => o.SelectedElement, (o, v) => o.SelectedElement = v);
    
    public static readonly DirectProperty<ElementSelector, string> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, string>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v);

    public static readonly DirectProperty<ElementSelector, bool> ShowAllProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, bool>(
            nameof(ShowAll), o => o.ShowAll, (o, v) => o.ShowAll = v);

    #endregion

    public IEnumerable<Element> Elements => _elements;
    
    public Element? SelectedElement
    {
        get => _selectedElement;
        set => SetAndRaise(SelectedElementProperty, ref _selectedElement, value);
    }

    public string FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }
    
    public bool ShowAll
    {
        get => _showAll;
        set => SetAndRaise(ShowAllProperty, ref _showAll, value);
    }
    
    private void OnShowAllChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not bool showAll) return;

        _source.Clear();

        if (showAll)
        {
            _source.AddRange(Element.List);
            return;
        }
        
        _source.AddRange(Element.List.Where(e => e.IsComponent));
    }
    
    private void OnFilterTextChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var value = args.NewValue?.ToString();
        
        _source.Clear();
        
        if (!string.IsNullOrEmpty(value))
        {
            _source.AddRange(Element.List.Where(e => e.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
            return;
        }
        
        _source.AddRange(Element.List.Where(e => e.IsComponent));
        ShowAll = false;
    }
    
    //Not sure if there is a better way to do this
    private void OnSelectedElementChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not Element) return;
        
        //If this is in a flyout menu then close
        var popup = this.FindLogicalAncestorOfType<Popup>();
        if (popup is null) return;
        popup.IsOpen = false;
    }
}