using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Pages.Projects;
using AutoSpex.Client.Pages.Specs;
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

    [ObservableProperty] private bool _isEditing;

    [RelayCommand]
    private void Open()
    {
        Navigator.Navigate(() => new NodePageModel(this));
    }

    [RelayCommand]
    private void OpenInTab()
    {
        var newTab = new KeyValuePair<string, object>("NewTab", true);
        Navigator.Navigate(() => new NodePageModel(this), newTab);
    }

    [RelayCommand]
    private async Task AddFolder()
    {
        var folder = new NodeObserver(Node.NewFolder());
        Nodes.Add(folder);

        var result = await Messenger.Send(new NodeCreateRequest(folder));

        if (result.IsFailed)
        {
            Nodes.Remove(folder);
            return;
        }
        
        var inFocus = new KeyValuePair<string, object>("InFocus", true);
        await Navigator.Navigate(() => new NodePageModel(this), inFocus);
    }

    [RelayCommand]
    private async Task AddSpec()
    {
        var spec = new NodeObserver(Node.NewSpec());
        Nodes.Add(spec);

        var result = await Messenger.Send(new NodeCreateRequest(spec));

        if (result.IsFailed)
        {
            Nodes.Remove(spec);
            return;
        }
        
        var inFocus = new KeyValuePair<string, object>("InFocus", true);
        await Navigator.Navigate(() => new NodePageModel(this), inFocus);
    }

    [RelayCommand]
    private async Task RenameNode(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        var previous = Name;
        Name = name;

        var reply = await Messenger.Send(new NodeRenameRequest(this));
        if (reply.IsFailed)
        {
            Name = previous;
            return;
        }

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

        var result = await Messenger.Send(new NodeDeleteRequest(this));
        if (result.IsFailed) return;
        Parent?.Nodes.Remove(this);
    }

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;
}