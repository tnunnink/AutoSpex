using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class DeletePrompt : TemplatedControl
{
    public static readonly StyledProperty<string?> ItemNameProperty =
        AvaloniaProperty.Register<DeletePrompt, string?>(
            nameof(ItemName));

    public string? ItemName
    {
        get => GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    public static readonly DirectProperty<DeletePrompt, string?> TitleProperty =
        AvaloniaProperty.RegisterDirect<DeletePrompt, string?>(
            nameof(Title), o => o.Title, (o, v) => o.Title = v);

    private string? _title;

    public string? Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemNameProperty)
        {
            Title = $"Delete {ItemName}?";
        }
    }
}