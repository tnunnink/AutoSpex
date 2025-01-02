using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.Properties;

public static class Icon
{
    /// <summary>
    /// Identifies the <seealso cref="ThemeProperty"/> avalonia attached property.
    /// </summary>
    public static readonly AttachedProperty<ControlTheme> ThemeProperty =
        AvaloniaProperty.RegisterAttached<PathIcon, ContentControl, ControlTheme>("Theme");

    static Icon()
    {
        /*ThemeProperty.Changed.Subscribe(HandleChange);*/
    }

    /// <summary>
    /// Accessor for attached property <see cref="ThemeProperty"/>
    /// </summary>
    public static ControlTheme GetTheme(ContentControl target)
    {
        return target.GetValue(ThemeProperty);
    }

    /// <summary>
    /// Accessor for attached property <see cref="ThemeProperty"/>
    /// </summary>
    public static void SetTheme(ContentControl target, ControlTheme value)
    {
        target.SetValue(ThemeProperty, value);
    }

    /*private static void HandleChange(AvaloniaPropertyChangedEventArgs<ControlTheme> args)
    {
        if (args.Sender is not ContentControl target)
        {
            return;
        }

        target.Content = new PathIcon
        {
            Theme = args.GetNewValue<ControlTheme>()
        };
    }*/
}