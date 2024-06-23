using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class NodePath : TemplatedControl
{
    private NodeObserver? _node;

    public static readonly DirectProperty<NodePath, NodeObserver?> NodeProperty =
        AvaloniaProperty.RegisterDirect<NodePath, NodeObserver?>(
            nameof(Node), o => o.Node, (o, v) => o.Node = v);

    public static readonly StyledProperty<bool> ShowNodeIconProperty =
        AvaloniaProperty.Register<NodePath, bool>(
            nameof(ShowNodeIcon), true);

    public NodeObserver? Node
    {
        get => _node;
        set => SetAndRaise(NodeProperty, ref _node, value);
    }

    public bool ShowNodeIcon
    {
        get => GetValue(ShowNodeIconProperty);
        set => SetValue(ShowNodeIconProperty, value);
    }

    public ObservableCollection<string> Parents { get; } = [];
    public string Target { get; private set; } = string.Empty;
    public string NodeType { get; private set; } = string.Empty;


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NodeProperty)
            UpdatePath(change.GetNewValue<NodeObserver?>());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePath(Node);
    }

    private void UpdatePath(NodeObserver? node)
    {
        Parents.Clear();
        Target = string.Empty;

        if (node is null) return;

        var parents = node.Model.Ancestors().Select(a => a.Name);
        Parents.Refresh(parents);
        Target = node.Name;
        NodeType = node.Type.Name;
    }
}