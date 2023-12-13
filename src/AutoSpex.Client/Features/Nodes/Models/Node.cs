using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Nodes;

public partial class Node : ObservableObject
{
    public Node(string name, NodeType nodeType, Node? parent = null)
    {
        NodeId = Guid.NewGuid();
        ParentId = parent?.NodeId ?? Guid.Empty;
        Parent = parent;
        Name = name;
        NodeType = nodeType;
        Depth = 0;
        Ordinal = 0;
        Description = string.Empty;
    }
    
    public Node(dynamic record)
    {
        NodeId = record.NodeId;
        ParentId = record.ParentId;
        Parent = record.Parent;
        NodeType = Enum.Parse<NodeType>(record.NodeType.ToString());
        Name = record.Name.ToString();
        Depth = record.Depth;
        Ordinal = record.Ordinal;
        Description = record.Description;
    }

    public Guid NodeId { get; }
    public Guid ParentId { get; private set; }
    public Node? Parent { get; private set; }

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

    [ObservableProperty] private bool _isProjectNode;
    [ObservableProperty] private bool _isFolderNode;
    [ObservableProperty] private bool _isSpecificationNode;

    public void AssignParent(Node? parent)
    {
        ParentId = parent?.NodeId ?? default;
        Parent = parent;
    }

    public void Relocate(long depth, long ordinal)
    {
        Depth = depth;
        Ordinal = ordinal;
    }

    partial void OnNodeTypeChanged(NodeType value)
    {
        IsProjectNode = value == NodeType.Collection;
        IsFolderNode = value == NodeType.Folder;
        IsSpecificationNode = value == NodeType.Spec;
    }

    public bool FilterPath(string text)
    {
        var descendents = Enumerable.Select<Node, bool>(Nodes, n => n.FilterPath(text)).Where(r => r).ToList().Count;

        if (string.IsNullOrEmpty(text))
        {
            IsVisible = true;
            IsExpanded = false;
        }
        else
        {
            IsVisible = Name.Contains(text) || descendents > 0;
            IsExpanded = IsVisible && (IsFolderNode || IsProjectNode);
        }

        return IsVisible;
    }
}