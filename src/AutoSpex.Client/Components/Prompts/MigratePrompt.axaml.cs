using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class MigratePrompt : TemplatedControl
{
    public static readonly StyledProperty<string?> ProjectNameProperty =
        AvaloniaProperty.Register<MigratePrompt, string?>(
            nameof(ProjectName));

    public string? ProjectName
    {
        get => GetValue(ProjectNameProperty);
        set => SetValue(ProjectNameProperty, value);
    }
}