using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<ControlTheme?> IconProperty =
        AvaloniaProperty.Register<IconButton, ControlTheme?>(
            nameof(Icon));

    public ControlTheme? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}