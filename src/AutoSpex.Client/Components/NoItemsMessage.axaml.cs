using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

public class NoItemsMessage : TemplatedControl
{
    public static readonly StyledProperty<ControlTheme> HeaderIconProperty =
        AvaloniaProperty.Register<NoItemsMessage, ControlTheme>(
            nameof(HeaderIcon));

    public static readonly StyledProperty<string> HeaderTextProperty =
        AvaloniaProperty.Register<NoItemsMessage, string>(
            nameof(HeaderText));

    public static readonly StyledProperty<ControlTheme> MessageIconProperty =
        AvaloniaProperty.Register<NoItemsMessage, ControlTheme>(
            nameof(MessageIcon));

    public static readonly StyledProperty<string> MessageTextProperty =
        AvaloniaProperty.Register<NoItemsMessage, string>(
            nameof(MessageText));

    public static readonly StyledProperty<string?> CustomMessageProperty =
        AvaloniaProperty.Register<NoItemsMessage, string?>(
            nameof(CustomMessage));

    public ControlTheme HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public string HeaderText
    {
        get => GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
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

    public string? CustomMessage
    {
        get => GetValue(CustomMessageProperty);
        set => SetValue(CustomMessageProperty, value);
    }
}