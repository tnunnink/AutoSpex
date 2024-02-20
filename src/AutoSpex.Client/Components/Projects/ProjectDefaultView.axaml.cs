using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class ProjectDefaultView : TemplatedControl
{
    private ICommand? _createCommand;
    private ICommand? _openCommand;
    
    public static readonly DirectProperty<ProjectDefaultView, ICommand?> CreateCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectDefaultView, ICommand?>
            (nameof(CreateCommand), o => o.CreateCommand, (o, v) => o.CreateCommand = v);
    
    public static readonly DirectProperty<ProjectDefaultView, ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectDefaultView, ICommand?>
            (nameof(OpenCommand), o => o.OpenCommand, (o, v) => o.OpenCommand = v);
    
    public ICommand? CreateCommand
    {
        get => _createCommand;
        set => SetAndRaise(CreateCommandProperty, ref _createCommand, value);
    }
    
    public ICommand? OpenCommand
    {
        get => _openCommand;
        set => SetAndRaise(OpenCommandProperty, ref _openCommand, value);
    }
}