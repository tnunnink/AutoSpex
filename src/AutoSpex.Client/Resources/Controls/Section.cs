using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace AutoSpex.Client.Resources.Controls;

public class Section : HeaderedContentControl
{
    #region Properties

    public static readonly StyledProperty<bool> ShowContentProperty =
        AvaloniaProperty.Register<Section, bool>(
            nameof(ShowContent), defaultValue: true);

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<Section, object?>(
            nameof(Icon));

    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<Section, IDataTemplate?>(
            nameof(IconTemplate));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<Section, object?>(
            nameof(Description));

    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<Section, IDataTemplate?>(
            nameof(DescriptionTemplate));

    public static readonly StyledProperty<object?> ActionProperty =
        AvaloniaProperty.Register<Section, object?>(
            nameof(Action));

    public static readonly StyledProperty<IDataTemplate?> ActionTemplateProperty =
        AvaloniaProperty.Register<Section, IDataTemplate?>(
            nameof(ActionTemplate));

    #endregion

    public bool ShowContent
    {
        get => GetValue(ShowContentProperty);
        set => SetValue(ShowContentProperty, value);
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
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