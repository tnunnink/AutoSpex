using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

[PseudoClasses(PcChanged)]
public class DetailTab : TabItem
{
    private const string PcChanged = ":changed";

    static DetailTab()
    {
        IsChangedProperty.Changed.AddClassHandler<DetailTab>((control, args) =>
            control.OnIsChangedPropertyChanged(args));
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DetailTab, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<ControlTheme?> IconProperty =
        AvaloniaProperty.Register<DetailTab, ControlTheme?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsChangedProperty =
        AvaloniaProperty.Register<DetailTab, bool>(nameof(IsChanged));

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