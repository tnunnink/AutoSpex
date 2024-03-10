using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using L5Sharp.Core;

namespace AutoSpex.Client.Components;

public class ElementList : TemplatedControl
{
    public static readonly DirectProperty<ElementList, ObservableCollection<LogixElement>> ElementSourceProperty =
        AvaloniaProperty.RegisterDirect<ElementList, ObservableCollection<LogixElement>>(
            nameof(ElementSource), o => o.ElementSource, (o, v) => o.ElementSource = v, unsetValue: []);

    public static readonly DirectProperty<ElementList, LogixElement?> SelectedElementProperty =
        AvaloniaProperty.RegisterDirect<ElementList, LogixElement?>(
            nameof(SelectedElement), o => o.SelectedElement, (o, v) => o.SelectedElement = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    private ObservableCollection<LogixElement> _elementSource = [];
    private LogixElement? _selectedElement;

    public ObservableCollection<LogixElement> ElementSource
    {
        get => _elementSource;
        set => SetAndRaise(ElementSourceProperty, ref _elementSource, value);
    }

    public LogixElement? SelectedElement
    {
        get => _selectedElement;
        set => SetAndRaise(SelectedElementProperty, ref _selectedElement, value);
    }
}