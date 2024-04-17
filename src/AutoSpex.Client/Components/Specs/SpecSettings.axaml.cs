using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SpecSettings : TemplatedControl
{
    public static readonly DirectProperty<SpecSettings, SpecObserver?> SpecProperty =
        AvaloniaProperty.RegisterDirect<SpecSettings, SpecObserver?>(
            nameof(Spec), o => o.Spec, (o, v) => o.Spec = v);

    private SpecObserver? _spec;

    public SpecObserver? Spec
    {
        get => _spec;
        set => SetAndRaise(SpecProperty, ref _spec, value);
    }
}