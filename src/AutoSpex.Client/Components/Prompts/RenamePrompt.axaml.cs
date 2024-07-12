using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class RenamePrompt : TemplatedControl
{
    public static readonly DirectProperty<RenamePrompt, Observer?> ObserverProperty =
        AvaloniaProperty.RegisterDirect<RenamePrompt, Observer?>(
            nameof(Observer), o => o.Observer, (o, v) => o.Observer = v);

    private Observer? _observer;

    public Observer? Observer
    {
        get => _observer;
        set => SetAndRaise(ObserverProperty, ref _observer, value);
    }
}