using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using L5Spex.Client.Common;
using L5Spex.Engine.Enumerations;

namespace L5Spex.Client.Observers;

public partial class NodeObserver : ObservableObject
{
    private NodeType _nodeType;
    
    public NodeObserver(NodePath path, NodeType type)
    {
        Name = path;
        NodeType = type;
    }

    public NodeObserver(dynamic record)
    {
        NodeId = record.NodeId;
        ParentId = record.ParentId;
        Name = record.Name.ToString();
        NodeType = Enum.Parse<NodeType>(record.NodeType.ToString());
        Ordinal = record.Ordinal;
    }

    public Guid NodeId { get; }
    public Guid? ParentId { get; }
    
    public NodeType NodeType
    {
        get => _nodeType;
        set
        {
            SetProperty(ref _nodeType, value);
            IsProjectNode = value == NodeType.Project;
            IsFolderNode = value == NodeType.Folder;
            IsSpecificationNode = value == NodeType.Specification;
        }
    }

    [ObservableProperty] private string _name;

    [ObservableProperty] private long _ordinal;

    [ObservableProperty] private ResultType _result;

    [ObservableProperty] private ObservableCollection<NodeObserver> _nodes = new();

    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isEditing;
    
    [ObservableProperty] private bool _isProjectNode;
    [ObservableProperty] private bool _isFolderNode;
    [ObservableProperty] private bool _isSpecificationNode;
    
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
            IsExpanded = IsVisible && (IsFolderNode || IsProjectNode);
        }

        return IsVisible;
    }
}