using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class DetailsDefaultView : TemplatedControl
{
    public static readonly DirectProperty<DetailsDefaultView, ICommand?> AddCollectionCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsDefaultView, ICommand?>(
            nameof(AddCollectionCommand), o => o.AddCollectionCommand, (o, v) => o.AddCollectionCommand = v);
    
    public static readonly DirectProperty<DetailsDefaultView, ICommand?> AddSpecCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsDefaultView, ICommand?>(
            nameof(AddSpecCommand), o => o.AddSpecCommand, (o, v) => o.AddSpecCommand = v);

    public static readonly DirectProperty<DetailsDefaultView, ICommand?> AddSourceCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsDefaultView, ICommand?>(
            nameof(AddSourceCommand), o => o.AddSourceCommand, (o, v) => o.AddSourceCommand = v);

    public static readonly DirectProperty<DetailsDefaultView, ICommand?> AddRunnerCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsDefaultView, ICommand?>(
            nameof(AddRunnerCommand), o => o.AddRunnerCommand, (o, v) => o.AddRunnerCommand = v);

    private ICommand? _addCollectionCommand;
    private ICommand? _addSpecCommand;
    private ICommand? _addSourceCommand;
    private ICommand? _addRunnerCommand;
    
    public ICommand? AddCollectionCommand
    {
        get => _addCollectionCommand;
        set => SetAndRaise(AddCollectionCommandProperty, ref _addCollectionCommand, value);
    }

    public ICommand? AddSpecCommand
    {
        get => _addSpecCommand;
        set => SetAndRaise(AddSpecCommandProperty, ref _addSpecCommand, value);
    }

    public ICommand? AddSourceCommand
    {
        get => _addSourceCommand;
        set => SetAndRaise(AddSourceCommandProperty, ref _addSourceCommand, value);
    }
    
    public ICommand? AddRunnerCommand
    {
        get => _addRunnerCommand;
        set => SetAndRaise(AddRunnerCommandProperty, ref _addRunnerCommand, value);
    }
}