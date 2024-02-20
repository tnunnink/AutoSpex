using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SourceList : TemplatedControl
{
    public static readonly DirectProperty<SourceList, ObservableCollection<SourceObserver>> SourcesProperty =
        AvaloniaProperty.RegisterDirect<SourceList, ObservableCollection<SourceObserver>>(
            nameof(Sources), o => o.Sources, (o, v) => o.Sources = v);

    public static readonly DirectProperty<SourceList, ICommand?> AddSourceCommandProperty =
        AvaloniaProperty.RegisterDirect<SourceList, ICommand?>(
            nameof(AddSourceCommand), o => o.AddSourceCommand, (o, v) => o.AddSourceCommand = v);


    private ObservableCollection<SourceObserver> _sources = [];
    private ICommand? _addSourceCommand;

    public ObservableCollection<SourceObserver> Sources
    {
        get => _sources;
        set => SetAndRaise(SourcesProperty, ref _sources, value);
    }
    
    public ICommand? AddSourceCommand
    {
        get => _addSourceCommand;
        set => SetAndRaise(AddSourceCommandProperty, ref _addSourceCommand, value);
    }
}