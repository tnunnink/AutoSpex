using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class NodeNotFound : TemplatedControl
{
    public static readonly DirectProperty<NodeNotFound, string?> NodeNameProperty =
        AvaloniaProperty.RegisterDirect<NodeNotFound, string?>(
            nameof(NodeName), o => o.NodeName, (o, v) => o.NodeName = v);

    public static readonly DirectProperty<NodeNotFound, Guid> NodeIdProperty =
        AvaloniaProperty.RegisterDirect<NodeNotFound, Guid>(
            nameof(NodeId), o => o.NodeId, (o, v) => o.NodeId = v);
    
    public static readonly DirectProperty<NodeNotFound, ICommand?> RemoveCommandProperty =
        AvaloniaProperty.RegisterDirect<NodeNotFound, ICommand?>(
            nameof(RemoveCommand), o => o.RemoveCommand, (o, v) => o.RemoveCommand = v);
    

    private Guid _nodeId;
    private string? _nodeName;
    private ICommand? _removeCommand;
    
    
    public string? NodeName
    {
        get => _nodeName;
        set => SetAndRaise(NodeNameProperty, ref _nodeName, value);
    }

    public Guid NodeId
    {
        get => _nodeId;
        set => SetAndRaise(NodeIdProperty, ref _nodeId, value);
    }

    public ICommand? RemoveCommand
    {
        get => _removeCommand;
        set => SetAndRaise(RemoveCommandProperty, ref _removeCommand, value);
    }
}