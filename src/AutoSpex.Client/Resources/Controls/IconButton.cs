using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.Controls;

public class IconButton : Button
{
    private ControlTheme? _icon;

    public static readonly DirectProperty<IconButton, ControlTheme?> IconProperty =
        AvaloniaProperty.RegisterDirect<IconButton, ControlTheme?>(
            nameof(Icon), o => o.Icon, (o, v) => o.Icon = v);
    
    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<IconButton, int>(nameof(IconSize), defaultValue: 30);

    public ControlTheme? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }

    public int IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}