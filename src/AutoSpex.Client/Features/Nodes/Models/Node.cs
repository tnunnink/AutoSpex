using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Nodes;

public partial class Node : ObservableObject
{
    public Node()
    {
        NodeId = Guid.NewGuid();
        ParentId = Guid.Empty;
        Parent = default;
        Name = string.Empty;
        Feature = Feature.Specifications;
        NodeType = NodeType.Collection;
        Depth = 0;
        Ordinal = 0;
        Nodes = new ObservableCollection<Node>();
    }

    public Guid NodeId { get; private init; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }

    [ObservableProperty] private Feature _feature;

    [ObservableProperty] private NodeType _nodeType;

    [ObservableProperty] private string _name;

    [ObservableProperty] private long _depth;

    [ObservableProperty] private long _ordinal;

    [ObservableProperty] private ObservableCollection<Node> _nodes;

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isVisible = true;

    public Breadcrumb Breadcrumb => new(this, CrumbType.Target);

    [ObservableProperty] private bool _isEditing;

    public static Node SpecCollection(string name)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Feature = Feature.Specifications,
            NodeType = NodeType.Collection,
            Name = name
        };
    }

    public static Node SourceCollection(string name)
    {
        return new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = Guid.Empty,
            Parent = default,
            Feature = Feature.Sources,
            NodeType = NodeType.Collection,
            Name = name
        };
    }

    public void AddNode(Node node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));

        if (node.Feature != Feature)
            throw new InvalidOperationException(
                $"Can not add node to tree of different feature. Tree: {Feature}; Node: {node.Feature}");

        node.Parent = this;
        node.ParentId = NodeId;
        Nodes.Add(node);
    }

    public Node NewFolder(string? name = default)
    {
        name ??= "New Folder";

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature,
            NodeType = NodeType.Folder,
            Name = name,
            Depth = Depth + 1
        };
        
        return node;
    }

    public Node NewSpec(string? name = default)
    {
        name ??= "New Spec";

        var node = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature.Specifications,
            NodeType = NodeType.Spec,
            Name = name,
            Depth = Depth + 1
        };
        
        return node;
    }

    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;

    public override int GetHashCode() => NodeId.GetHashCode();

    public static bool operator ==(Node? left, Node? right) => Equals(left, right);

    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);

    public bool FilterPath(string text)
    {
        var descendents = Nodes.Select(n => n.FilterPath(text)).Where(r => r).ToList().Count;

        if (string.IsNullOrEmpty(text))
        {
            IsVisible = true;
            IsExpanded = false;
        }
        else
        {
            IsVisible = Name.Contains(text) || descendents > 0;
            IsExpanded = IsVisible && NodeType == NodeType.Folder || NodeType == NodeType.Collection;
        }

        return IsVisible;
    }
}