using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Client.Components;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Pages;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

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

    [Required]
    [MaxLength(100)]
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (n, s) => n.Name = s, validate: true);
    }

    public ObserverCollection<Node, NodeObserver> Nodes { get; }
    public SpecObserver? Spec => Model.Spec is not null ? new SpecObserver(Model.Spec) : default;
    
    public Breadcrumb Breadcrumb => new(this, CrumbType.Target);

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isVisible = true;
    
    [ObservableProperty] private bool _isEditing;

    [RelayCommand]
    private void Open()
    {
        Navigator.NavigateTo<DetailsPageModel>(() => new NodePageModel(this));
    }

    [RelayCommand]
    private void OpenInTab()
    {
        var inNewTab = new KeyValuePair<string, object>("InNewTab", true);
        Navigator.NavigateTo<DetailsPageModel>(() => new NodePageModel(this), inNewTab);
    }

    [RelayCommand]
    private async Task AddFolder(NodeObserver parent)
    {
        var folder = new NodeObserver(Node.NewFolder());
        parent.Nodes.Add(folder);

        var result = await Messenger.Send(new NodeCreateRequest(folder));

        if (result.IsFailed)
        {
            parent.Nodes.Remove(folder);
            return;
        }

        folder.IsSelected = true;
        var inFocus = new KeyValuePair<string, object>("InFocus", true);
        await Navigator.NavigateTo<DetailsPageModel>(() => new NodePageModel(this), inFocus);
    }
    
    [RelayCommand]
    private async Task AddSpec(NodeObserver parent)
    {
        var spec = new NodeObserver(Node.NewSpec());
        parent.Nodes.Add(spec);

        var result = await Messenger.Send(new NodeCreateRequest(spec));

        if (result.IsFailed)
        {
            parent.Nodes.Remove(spec);
            return;
        }

        spec.IsSelected = true;
        var inFocus = new KeyValuePair<string, object>("InFocus", true);
        await Navigator.NavigateTo<DetailsPageModel>(() => new NodePageModel(this), inFocus);
    }

    [RelayCommand]
    private void RenameNode(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        Name = name;
        Messenger.Send(new NodeRenamedMessage(this));
    }

    [RelayCommand]
    private async Task DeleteNode()
    {
        var answer = await UserPromptBuilder.Configure()
            .WithHeaderContent($"Delete {NodeType} {Name}?")
            .WithContent($"Deleting this {NodeType} will remove it an all children from the collection.")
            .WithStandardButtons(MessageBoxButtons.YesNo)
            .Show();

        if (answer != MessageBoxResult.Yes) return;

        var result = await Messenger.Send(new NodeDeleteRequest(NodeId));

        if (result.IsSuccess)
        {
            if (Parent is null)
            {
                //we have to send collection as message.
                return;
            }

            Parent.Nodes.Remove(this);
        }
    }

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;

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
            IsExpanded = IsVisible && (NodeType == NodeType.Folder || NodeType == NodeType.Collection);
        }

        return IsVisible;
    }
}