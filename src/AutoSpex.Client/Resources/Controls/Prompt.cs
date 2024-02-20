using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Resources.Controls;

public class Prompt : HeaderedContentControl
{
    #region Properties

    public static readonly StyledProperty<SolidColorBrush?> HeaderBackgroundProperty =
        AvaloniaProperty.Register<Prompt, SolidColorBrush?>(
            nameof(HeaderBackground));

    public static readonly StyledProperty<SolidColorBrush> HeaderForegroundProperty =
        AvaloniaProperty.Register<Prompt, SolidColorBrush>(
            nameof(HeaderForeground));

    public static readonly StyledProperty<ControlTheme> HeaderIconProperty =
        AvaloniaProperty.Register<Prompt, ControlTheme>(
            nameof(HeaderIcon));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Prompt, object?>(
            nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<Prompt, IDataTemplate?>(
            nameof(FooterTemplate));

    public static readonly StyledProperty<SolidColorBrush> FooterBackgroundProperty =
        AvaloniaProperty.Register<Prompt, SolidColorBrush>(
            nameof(FooterBackground));

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<Prompt, ICommand?>(
            nameof(CloseCommand));

    public static readonly StyledProperty<string?> ActionButtonTextProperty =
        AvaloniaProperty.Register<Prompt, string?>(
            nameof(ActionButtonText), defaultValue: "Yes");
    
    public static readonly StyledProperty<ControlTheme?> ActionButtonThemeProperty =
        AvaloniaProperty.Register<Prompt, ControlTheme?>(
            nameof(ActionButtonTheme));

    public static readonly StyledProperty<object?> ActionCommandParameterProperty =
        AvaloniaProperty.Register<Prompt, object?>(
            nameof(ActionCommandParameter));
    
    public static readonly StyledProperty<string?> CancelButtonTextProperty =
        AvaloniaProperty.Register<Prompt, string?>(
            nameof(CancelButtonText), defaultValue: "Cancel");

    public static readonly StyledProperty<ControlTheme?> CancelButtonThemeProperty =
        AvaloniaProperty.Register<Prompt, ControlTheme?>(
            nameof(CancelButtonTheme));

    public static readonly StyledProperty<object?> CancelCommandParameterProperty =
        AvaloniaProperty.Register<Prompt, object?>(
            nameof(CancelCommandParameter));
    
    public static readonly StyledProperty<string?> AlternateButtonTextProperty =
        AvaloniaProperty.Register<Prompt, string?>(
            nameof(AlternateButtonText), defaultValue: "Alternate");

    public static readonly StyledProperty<ControlTheme?> AlternateButtonThemeProperty =
        AvaloniaProperty.Register<Prompt, ControlTheme?>(
            nameof(AlternateButtonTheme));

    public static readonly StyledProperty<object?> AlternateCommandParameterProperty =
        AvaloniaProperty.Register<Prompt, object?>(
            nameof(AlternateCommandParameter));
    
    public static readonly StyledProperty<bool> UseButtonPanelProperty =
        AvaloniaProperty.Register<Prompt, bool>(
            nameof(UseButtonPanel), defaultValue:true);

    public static readonly StyledProperty<bool> UseAlternateButtonProperty =
        AvaloniaProperty.Register<Prompt, bool>(
            nameof(UseAlternateButton));

    #endregion

    public Prompt()
    {
        CloseCommand = new RelayCommand<object>(CloseWindow);
    }

    public SolidColorBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public SolidColorBrush HeaderForeground
    {
        get => GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    public ControlTheme HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
    
    public SolidColorBrush FooterBackground
    {
        get => GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        init => SetValue(CloseCommandProperty, value);
    }

    public string? ActionButtonText
    {
        get => GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }
    
    public ControlTheme? ActionButtonTheme
    {
        get => GetValue(ActionButtonThemeProperty);
        set => SetValue(ActionButtonThemeProperty, value);
    }

    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }
    
    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }
    
    public ControlTheme? CancelButtonTheme
    {
        get => GetValue(CancelButtonThemeProperty);
        set => SetValue(CancelButtonThemeProperty, value);
    }

    public object? CancelCommandParameter
    {
        get => GetValue(CancelCommandParameterProperty);
        set => SetValue(CancelCommandParameterProperty, value);
    }
    
    public string? AlternateButtonText
    {
        get => GetValue(AlternateButtonTextProperty);
        set => SetValue(AlternateButtonTextProperty, value);
    }
    
    public ControlTheme? AlternateButtonTheme
    {
        get => GetValue(AlternateButtonThemeProperty);
        set => SetValue(AlternateButtonThemeProperty, value);
    }

    public object? AlternateCommandParameter
    {
        get => GetValue(AlternateCommandParameterProperty);
        set => SetValue(AlternateCommandParameterProperty, value);
    }
    
    public bool UseButtonPanel
    {
        get => GetValue(UseButtonPanelProperty);
        set => SetValue(UseButtonPanelProperty, value);
    }

    public bool UseAlternateButton
    {
        get => GetValue(UseAlternateButtonProperty);
        set => SetValue(UseAlternateButtonProperty, value);
    }

    private void CloseWindow(object? result)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.Close(result);
    }
}