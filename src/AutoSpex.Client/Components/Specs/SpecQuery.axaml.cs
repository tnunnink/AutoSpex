using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SpecQuery : TemplatedControl
{
    public static readonly StyledProperty<string?> HeadingProperty =
        AvaloniaProperty.Register<SpecQuery, string?>(
            nameof(Heading));

    public static readonly StyledProperty<string?> InfoTextProperty =
        AvaloniaProperty.Register<SpecQuery, string?>(
            nameof(InfoText));

    public static readonly DirectProperty<SpecQuery, SpecObserver?> SpecProperty =
        AvaloniaProperty.RegisterDirect<SpecQuery, SpecObserver?>(
            nameof(Spec), o => o.Spec, (o, v) => o.Spec = v);

    private SpecObserver? _spec;

    public string? Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public string? InfoText
    {
        get => GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
    }

    public SpecObserver? Spec
    {
        get => _spec;
        set => SetAndRaise(SpecProperty, ref _spec, value);
    }
}