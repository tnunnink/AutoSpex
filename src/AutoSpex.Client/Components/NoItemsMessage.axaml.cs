using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

public class NoItemsMessage : TemplatedControl
{
    public static readonly StyledProperty<string> HeadingTextProperty =
        AvaloniaProperty.Register<NoItemsMessage, string>(
            nameof(HeadingText));

    public static readonly StyledProperty<ControlTheme> MessageIconProperty =
        AvaloniaProperty.Register<NoItemsMessage, ControlTheme>(
            nameof(MessageIcon));
    
    public static readonly StyledProperty<string> MessageTextProperty =
        AvaloniaProperty.Register<NoItemsMessage, string>(
            nameof(MessageText));

    public string HeadingText
    {
        get => GetValue(HeadingTextProperty);
        set => SetValue(HeadingTextProperty, value);
    }

    public ControlTheme MessageIcon
    {
        get => GetValue(MessageIconProperty);
        set => SetValue(MessageIconProperty, value);
    }
    
    public string MessageText
    {
        get => GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }
}