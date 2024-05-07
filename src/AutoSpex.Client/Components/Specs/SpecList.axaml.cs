using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class SpecList : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<SpecList, ObservableCollection<NodeObserver>> SpecsProperty =
        AvaloniaProperty.RegisterDirect<SpecList, ObservableCollection<NodeObserver>>(
            nameof(Specs), o => o.Specs, (o, v) => o.Specs = v);

    public static readonly DirectProperty<SpecList, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecList, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v);

    public static readonly DirectProperty<SpecList, ICommand?> RemoveCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecList, ICommand?>(
            nameof(RemoveCommand), o => o.RemoveCommand, (o, v) => o.RemoveCommand = v);

    public static readonly DirectProperty<SpecList, ICommand?> CopyCommandProperty =
        AvaloniaProperty.RegisterDirect<SpecList, ICommand?>(
            nameof(CopyCommand), o => o.CopyCommand, (o, v) => o.CopyCommand = v);

    #endregion


    private ObservableCollection<NodeObserver> _specs = [];
    private ICommand? _addCommand;
    private ICommand? _removeCommand;
    private ICommand? _copyCommand;

    public ObservableCollection<NodeObserver> Specs
    {
        get => _specs;
        set => SetAndRaise(SpecsProperty, ref _specs, value);
    }

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public ICommand? RemoveCommand
    {
        get => _removeCommand;
        set => SetAndRaise(RemoveCommandProperty, ref _removeCommand, value);
    }

    public ICommand? CopyCommand
    {
        get => _copyCommand;
        set => SetAndRaise(CopyCommandProperty, ref _copyCommand, value);
    }
}