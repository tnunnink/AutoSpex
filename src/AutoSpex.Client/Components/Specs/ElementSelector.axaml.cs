using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AutoSpex.Client.Components;

public class ElementSelector : TemplatedControl
{
    public static readonly DirectProperty<ElementSelector, Element?> ElementProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, Element?>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<ElementSelector, string?> TargetNameProperty =
        AvaloniaProperty.RegisterDirect<ElementSelector, string?>(
            nameof(TargetName), o => o.TargetName, (o, v) => o.TargetName = v);

    private Element? _element;
    private string? _targetName;

    public Element? Element
    {
        get => _element;
        set => SetAndRaise(ElementProperty, ref _element, value);
    }
    
    public string? TargetName
    {
        get => _targetName;
        set => SetAndRaise(TargetNameProperty, ref _targetName, value);
    }
}