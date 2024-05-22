using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class DeletePrompt : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DeletePrompt, string?>(
            nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<DeletePrompt, string?>(
            nameof(Message));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}