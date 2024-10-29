using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Styling;
// ReSharper disable MemberCanBePrivate.Global

namespace AutoSpex.Client.Resources.Controls;

public class ListView : ListBox
{
    #region Properties

    public static readonly StyledProperty<string?> FilterProperty =
        AvaloniaProperty.Register<ListView, string?>(
            nameof(Filter), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<ListView, object?>(
            nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<ListView, IDataTemplate?>(
            nameof(HeaderTemplate));

    public static readonly StyledProperty<bool> HasItemsProperty =
        AvaloniaProperty.Register<ListView, bool>(
            nameof(HasItems));

    public static readonly StyledProperty<ControlTheme> DefaultIconProperty =
        AvaloniaProperty.Register<ListView, ControlTheme>(
            nameof(DefaultIcon));

    public static readonly StyledProperty<string?> DefaultMessageProperty =
        AvaloniaProperty.Register<ListView, string?>(
            nameof(DefaultMessage));

    public static readonly StyledProperty<string?> DefaultCaptionProperty =
        AvaloniaProperty.Register<ListView, string?>(
            nameof(DefaultCaption));

    #endregion

    public string? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public bool HasItems
    {
        get => GetValue(HasItemsProperty);
        set => SetValue(HasItemsProperty, value);
    }

    public ControlTheme DefaultIcon
    {
        get => GetValue(DefaultIconProperty);
        set => SetValue(DefaultIconProperty, value);
    }

    public string? DefaultMessage
    {
        get => GetValue(DefaultMessageProperty);
        set => SetValue(DefaultMessageProperty, value);
    }

    public string? DefaultCaption
    {
        get => GetValue(DefaultCaptionProperty);
        set => SetValue(DefaultCaptionProperty, value);
    }
}