using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

[PseudoClasses(PcChanged)]
public class DetailsTab : TabItem
{
    private const string PcChanged = ":changed";

    static DetailsTab()
    {
        IsChangedProperty.Changed.AddClassHandler<DetailsTab>((control, args) =>
            control.OnIsChangedPropertyChanged(args));
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DetailsTab, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<ControlTheme?> IconProperty =
        AvaloniaProperty.Register<DetailsTab, ControlTheme?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsChangedProperty =
        AvaloniaProperty.Register<DetailsTab, bool>(nameof(IsChanged));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ControlTheme? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsChanged
    {
        get => GetValue(IsChangedProperty);
        set => SetValue(IsChangedProperty, value);
    }

    private void OnIsChangedPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not bool flag) return;
        PseudoClasses.Set(PcChanged, flag);
    }
}