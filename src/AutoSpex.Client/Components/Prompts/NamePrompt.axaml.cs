using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class NamePrompt : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NamePrompt, string?>(
            nameof(Title));

    public static readonly StyledProperty<Observer?> ObserverProperty =
        AvaloniaProperty.Register<NamePrompt, Observer?>(
            nameof(Observer));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Observer? Observer
    {
        get => GetValue(ObserverProperty);
        set => SetValue(ObserverProperty, value);
    }
}