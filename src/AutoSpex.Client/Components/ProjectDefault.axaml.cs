using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class ProjectDefault : TemplatedControl
{
    public static readonly DirectProperty<ProjectDefault, ICommand?> CreateCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectDefault, ICommand?>
            (nameof(CreateCommand), o => o.CreateCommand, (o, v) => o.CreateCommand = v);
    
    public static readonly DirectProperty<ProjectDefault, ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectDefault, ICommand?>
            (nameof(OpenCommand), o => o.OpenCommand, (o, v) => o.OpenCommand = v);


    private ICommand? _createCommand;
    public ICommand? CreateCommand
    {
        get => _createCommand;
        set => SetAndRaise(CreateCommandProperty, ref _createCommand, value);
    }
    
    private ICommand? _openCommand;
    public ICommand? OpenCommand
    {
        get => _openCommand;
        set => SetAndRaise(OpenCommandProperty, ref _openCommand, value);
    }
}