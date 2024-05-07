using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using L5Sharp.Core;

namespace AutoSpex.Client.Components;

public class ElementList : TemplatedControl
{
    public static readonly DirectProperty<ElementList, ObservableCollection<ElementObserver>> ElementsProperty =
        AvaloniaProperty.RegisterDirect<ElementList, ObservableCollection<ElementObserver>>(
            nameof(Elements), o => o.Elements, (o, v) => o.Elements = v, unsetValue: []);

    public static readonly DirectProperty<ElementList, ElementObserver?> SelectedElementProperty =
        AvaloniaProperty.RegisterDirect<ElementList, ElementObserver?>(
            nameof(SelectedElement), o => o.SelectedElement, (o, v) => o.SelectedElement = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    private ObservableCollection<ElementObserver> _elements = [];
    private ElementObserver? _selectedElement;

    public ObservableCollection<ElementObserver> Elements
    {
        get => _elements;
        set => SetAndRaise(ElementsProperty, ref _elements, value);
    }

    public ElementObserver? SelectedElement
    {
        get => _selectedElement;
        set => SetAndRaise(SelectedElementProperty, ref _selectedElement, value);
    }
}