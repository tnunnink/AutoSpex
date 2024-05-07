using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class DisconnectionPrompt : TemplatedControl
{
    public static readonly StyledProperty<string?> ProjectPathProperty =
        AvaloniaProperty.Register<DisconnectionPrompt, string?>(
            nameof(ProjectPath));

    public string? ProjectPath
    {
        get => GetValue(ProjectPathProperty);
        set => SetValue(ProjectPathProperty, value);
    }
}