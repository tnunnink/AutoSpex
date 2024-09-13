using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.Controls;

public class DefaultMessage : HeaderedContentControl
{
    public static readonly StyledProperty<ControlTheme> HeaderIconProperty =
        AvaloniaProperty.Register<DefaultMessage, ControlTheme>(
            nameof(HeaderIcon));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<DefaultMessage, string?>(
            nameof(Message));
    
    public static readonly StyledProperty<object?> ActionProperty =
        AvaloniaProperty.Register<Section, object?>(
            nameof(Action));

    public static readonly StyledProperty<IDataTemplate?> ActionTemplateProperty =
        AvaloniaProperty.Register<Section, IDataTemplate?>(
            nameof(ActionTemplate));

    public ControlTheme HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
    
    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    public IDataTemplate? ActionTemplate
    {
        get => GetValue(ActionTemplateProperty);
        set => SetValue(ActionTemplateProperty, value);
    }
}