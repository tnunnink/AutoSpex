using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.Controls;

public class DefaultMessage : TemplatedControl
{
    public static readonly StyledProperty<ControlTheme> HeaderIconProperty =
        AvaloniaProperty.Register<DefaultMessage, ControlTheme>(
            nameof(HeaderIcon));

    public static readonly StyledProperty<string> HeaderTextProperty =
        AvaloniaProperty.Register<DefaultMessage, string>(
            nameof(HeaderText));

    public static readonly StyledProperty<ControlTheme> MessageIconProperty =
        AvaloniaProperty.Register<DefaultMessage, ControlTheme>(
            nameof(MessageIcon));

    public static readonly StyledProperty<string> MessageTextProperty =
        AvaloniaProperty.Register<DefaultMessage, string>(
            nameof(MessageText));

    public static readonly StyledProperty<string?> CustomMessageProperty =
        AvaloniaProperty.Register<DefaultMessage, string?>(
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