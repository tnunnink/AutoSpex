using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SettingItem : ContentControl
{
    public static readonly DirectProperty<SettingItem, string?> LabelProperty =
        AvaloniaProperty.RegisterDirect<SettingItem, string?>(
            nameof(Label), o => o.Label, (o, v) => o.Label = v);

    public static readonly DirectProperty<SettingItem, string?> InfoTextProperty =
        AvaloniaProperty.RegisterDirect<SettingItem, string?>(
            nameof(InfoText), o => o.InfoText, (o, v) => o.InfoText = v);

    private string? _label;
    private string? _infoText;

    public string? InfoText
    {
        get => _infoText;
        set => SetAndRaise(InfoTextProperty, ref _infoText, value);
    }

    public string? Label
    {
        get => _label;
        set => SetAndRaise(LabelProperty, ref _label, value);
    }
}