using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class NodeHeader : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<NodeHeader, NodeObserver?> NodeProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, NodeObserver?>(
            nameof(Node), o => o.Node, (o, v) => o.Node = v);

    public static readonly DirectProperty<NodeHeader, ICommand?> RunCommandProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, ICommand?>(
            nameof(RunCommand), o => o.RunCommand, (o, v) => o.RunCommand = v);

    public static readonly DirectProperty<NodeHeader, ICommand?> SaveCommandProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, ICommand?>(
            nameof(SaveCommand), o => o.SaveCommand, (o, v) => o.SaveCommand = v);

    #endregion
    
    private NodeObserver? _node;
    private ICommand? _runCommand;
    private ICommand? _saveCommand;

    public NodeObserver? Node
    {
        get => _node;
        set => SetAndRaise(NodeProperty, ref _node, value);
    }
    
    public ICommand? RunCommand
    {
        get => _runCommand;
        set => SetAndRaise(RunCommandProperty, ref _runCommand, value);
    }

    public ICommand? SaveCommand
    {
        get => _saveCommand;
        set => SetAndRaise(SaveCommandProperty, ref _saveCommand, value);
    }
}