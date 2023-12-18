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
        Description = string.Empty;
    }

    public Guid NodeId { get; private init; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }

    [ObservableProperty] private Feature _feature;

    [ObservableProperty] private NodeType _nodeType;

    [ObservableProperty] private string _name;

    [ObservableProperty] private long _depth;

    [ObservableProperty] private long _ordinal;

    [ObservableProperty] private string? _description;

    [ObservableProperty] private ObservableCollection<Node> _nodes = new();

    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isVisible = true;
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
            Name = name,
            Description = string.Empty
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
            Name = name,
            Description = string.Empty
        };
    }

    public void AddNode(Node node)
    {
        if (node is null) throw new ArgumentNullException(nameof(node));
        
        if (node.Feature != Feature)
            throw new InvalidOperationException(
                $"Can not add node to tree of different feature. Feature: {Feature}; Node: {node.Feature}");

        node.Parent = this;
        node.ParentId = NodeId;
        Nodes.Add(node);
    }

    public Node AddFolder(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name can not be null or empty.");

        var folder = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature,
            NodeType = NodeType.Folder,
            Name = name,
            Depth = Depth + 1,
            Description = string.Empty
        };

        Nodes.Add(folder);
        return folder;
    }

    public Node AddSpec(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name can not be null or empty.");

        if (NodeType == NodeType.Collection || NodeType == NodeType.Folder)
            throw new InvalidOperationException("Can not add specification node to a non collection or folder node");

        var folder = new Node
        {
            NodeId = Guid.NewGuid(),
            ParentId = NodeId,
            Parent = this,
            Feature = Feature.Specifications,
            NodeType = NodeType.Spec,
            Name = name,
            Depth = Depth + 1,
            Description = string.Empty
        };

        Nodes.Add(folder);
        return folder;
    }

    public override bool Equals(object? obj) => obj is Node other && other.NodeId == NodeId;

    public override int GetHashCode() => NodeId.GetHashCode();

    public static bool operator ==(Node? left, Node? right) => Equals(left, right);

    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);

    public void Relocate(long depth, long ordinal)
    {
        Depth = depth;
        Ordinal = ordinal;
    }

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