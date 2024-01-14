using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features;

public partial class NodeObserver : Observer<Node>
{
    public NodeObserver(Node node) : base(node)
    {
        Nodes = new ObserverCollection<Node, NodeObserver>(Model.Nodes.ToList(),
            m => new NodeObserver(m),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.RemoveNode(n));
    }

    public Guid NodeId => Model.NodeId;
    public NodeObserver? Parent => Model.Parent is not null ? new NodeObserver(Model.Parent) : default;
    public NodeType NodeType => Model.NodeType;

    /*public NodeObserver? Collection =>*/

    [Required]
    [MaxLength(100)]
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (n, s) => n.Name = s, validate: true);
    }

    public DetailViewModel NewDetail()
    {
        if (NodeType == NodeType.Collection)
            return new CollectionViewModel(this);

        if (NodeType == NodeType.Folder)
            return new FolderViewModel(this);

        return new SpecViewModel(this);
    }

    public ObserverCollection<Node, NodeObserver> Nodes { get; private set; }

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isVisible = true;

    public Breadcrumb Breadcrumb => new(this, CrumbType.Target);

    [ObservableProperty] private bool _isEditing;

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;

    /*public bool FilterPath(string text)
    {
        var descendents = Node.Nodes.Select(n => n.FilterPath(text)).Where(r => r).ToList().Count;

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
    }*/
}