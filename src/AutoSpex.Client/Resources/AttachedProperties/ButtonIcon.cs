using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.AttachedProperties;

public class ButtonIcon : AvaloniaObject
{
    public static readonly AttachedProperty<ControlTheme> ThemeProperty =
        AvaloniaProperty.RegisterAttached<ButtonIcon, Button, ControlTheme>("Theme");

    public static void SetTheme(AvaloniaObject element, ControlTheme theme) => element.SetValue(ThemeProperty, theme);
    public static ControlTheme GetTheme(AvaloniaObject element) => element.GetValue(ThemeProperty);
}