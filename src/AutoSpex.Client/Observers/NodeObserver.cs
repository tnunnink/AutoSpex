using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[PublicAPI]
public partial class NodeObserver : Observer<Node>,
    IRecipient<Observer<Node>.Created>,
    IRecipient<Observer<Node>.Deleted>,
    IRecipient<NodeObserver.Renamed>,
    IRecipient<NodeObserver.Moved>,
    IRecipient<NodeObserver.GetSelected>
{
    public NodeObserver(Node node) : base(node)
    {
        Nodes = new ObserverCollection<Node, NodeObserver>(
            () => Model.Nodes.Select(n => new NodeObserver(n)).ToList(),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.RemoveNode(n),
            () => Model.ClearNodes());

        Track(nameof(Name));
    }

    public override Guid Id => Model.NodeId;
    public Guid ParentId => Model.ParentId;
    public NodeObserver? Parent => Model.Parent is not null ? new NodeObserver(Model.Parent) : default;
    public NodeType Type => Model.Type;
    public NodeType Feature => Model.Feature;
    public string Path => Model.Path;

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public ObserverCollection<Node, NodeObserver> Nodes { get; }
    public ObservableCollection<NodeObserver> Crumbs => new(Model.Ancestors().Select(n => new NodeObserver(n)));

    public ObservableCollection<NodeObserver> Descendents =>
        new(Model.Descendents().Where(n => n.Type != NodeType.Container).Select(n => new NodeObserver(n)));

    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isEditing;

    [ObservableProperty] private bool _isChecked = true;

    [ObservableProperty] private bool _isNew;

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return string.IsNullOrEmpty(filter) || Name.PassesFilter(filter) || Path.PassesFilter(filter);
    }

    /// <inheritdoc />
    public override string ToString() => Name;

    #region Commands

    [RelayCommand]
    private Task AddContainer() => AddNode(new NodeObserver(Node.NewContainer()) { IsNew = true });

    [RelayCommand]
    private Task AddSpec() => AddNode(new NodeObserver(Node.NewSpec()) { IsNew = true });

    [RelayCommand]
    private Task AddSource() => AddNode(new NodeObserver(Node.NewSource()) { IsNew = true });

    [RelayCommand]
    private Task AddRun() => AddNode(new NodeObserver(Node.NewRun()) { IsNew = true });

    private async Task AddNode(NodeObserver node)
    {
        //Add before sending request because this sets the parent correctly.
        Nodes.Add(node);

        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsFailed)
        {
            Nodes.Remove(node);
            return;
        }

        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        await Navigator.Navigate(node);
    }

    /// <inheritdoc />
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDeleteItem(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteNode(Id));
        if (result.IsFailed) return;

        Messenger.Send(new Deleted(this));
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        if (!IsSelected) return;

        var selected = Messenger.Send(new GetSelected()).Responses;

        if (selected.Count == 1)
        {
            var delete = await Prompter.PromptDeleteItem(Name);
            if (delete is not true) return;
        }
        else
        {
            var delete = await Prompter.PromptDeleteItems($"{selected.Count.ToString()} selected items");
            if (delete is not true) return;
        }

        var result = await Mediator.Send(new DeleteNodes(selected.Select(n => n.Id)));
        if (result.IsFailed) return;

        foreach (var deleted in selected)
            Messenger.Send(new Deleted(deleted));
    }

    [RelayCommand]
    private async Task Rename(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 100) return;

        Name = name;
        var rename = await Mediator.Send(new RenameNode(this));
        if (rename.IsFailed) return;

        IsEditing = false;
        IsNew = false;
        AcceptChanges(nameof(Name));
        Messenger.Send(new Renamed(this));
    }

    [RelayCommand]
    private void ResetName()
    {
        IsEditing = false;
        OnPropertyChanged(nameof(Name));
    }

    [RelayCommand]
    private async Task Move(IList<NodeObserver> nodes)
    {
        var result = await Mediator.Send(new MoveNodes(nodes.Select(n => n.Model), Id));
        if (result.IsFailed) return;

        Nodes.AddRange(nodes);

        foreach (var node in nodes)
            Messenger.Send(new Moved(node));
    }

    [RelayCommand]
    private async Task MoveSelected()
    {
        var selected = Messenger.Send(new GetSelected()).Responses;

        var parent = await Prompter.Show<NodeObserver?>(() => new SelectContainerPageModel(Feature));
        if (parent is null) return;

        var result = await Mediator.Send(new MoveNodes(selected.Select(n => n.Model), parent.Id));
        if (result.IsFailed) return;

        parent.Nodes.AddRange(selected);

        foreach (var node in selected)
            Messenger.Send(new Moved(node));
    }

    [RelayCommand]
    private void Edit()
    {
        IsEditing = true;
    }

    [RelayCommand]
    private async Task Run()
    {
        //Runs open the runner with the run.
        if (Feature == NodeType.Run)
        {
            await NavigateRunnerPage();
            return;
        }

        //Specs and Source (or their containers) open a run page configured with its nodes.
        await NavigateRunPage();
    }

    #endregion

    #region MessageHandlers

    /// <summary>
    /// Will handle the creation of a node. Parent nodes need to ensure the object is added if it belongs as a child.
    /// Also, this is where we want to sort the children to ensure proper ordering.
    /// </summary>
    public void Receive(Created message)
    {
        if (message.Observer is not NodeObserver node) return;

        if (Id != node.ParentId) return;
        if (Nodes.Any(n => n.Id == node.Id)) return;

        Nodes.Add(node);
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Will handle the removal of the node and/or child nodes as based on which observer object is received.
    /// If the delete node is a child to this node then we want it remove to update the underlying model object.
    /// We also want to propagate deleted messages to children to ensure the UI responds correctly.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not NodeObserver node) return;

        if (Nodes.Any(n => n.Id == node.Id))
        {
            Nodes.Remove(node);
            return;
        }

        if (Id != node.Id) return;
        Nodes.ToList().ForEach(n => Messenger.Send(new Deleted(n)));
        Messenger.UnregisterAll(this);
    }

    /// <summary>
    /// Handles the observer renamed message by updating this object's name if it is not the sender of the
    /// message. If this object is updated then it in turn sends a renamed message.
    /// </summary>
    public void Receive(Renamed message)
    {
        var node = message.Node;

        //If I am the parent to the renamed node then sort my children to ensure proper ordering.
        if (Id == node.ParentId)
        {
            Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
            return;
        }

        //If this is the same instance or not the changed node then return.
        if (ReferenceEquals(this, node)) return;
        if (Id != node.Id) return;

        //Otherwise make sure the name is synced. 
        if (Name != node.Name)
        {
            Name = node.Name;
            return;
        }

        //Notify property change regardless to update the UI.
        OnPropertyChanged(nameof(Name));
    }

    /// <summary>
    /// Handles the observer moved message by removing from the old parent and adding to the new parent if the sent
    /// node is a child of the old or new node.
    /// </summary>
    public void Receive(Moved message)
    {
        if (Nodes.Contains(message.Node) && Id != message.Node.ParentId)
            Nodes.Remove(message.Node);

        if (!Nodes.Contains(message.Node) && Id == message.Node.ParentId)
            Nodes.Add(message.Node);
    }

    /// <summary>
    /// Handles the <see cref="GetSelected"/> request message sent from a node. If this is a selected node then
    /// and the responses does not container it, reply with this node object. IsSelected is assumed to be used by the
    /// main navigation tree of the application.
    /// </summary>
    public void Receive(GetSelected message)
    {
        if (!IsSelected) return;
        if (message.Responses.Contains(this)) return;
        message.Reply(this);
    }

    #endregion

    /// <summary>
    /// This will force the editing mode to end when the item loses selection.
    /// </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        if (IsEditing && !value)
        {
            IsEditing = false;
        }
    }

    /// <summary>
    /// Configures a new temp run node and runner with this node (and all descendant nodes) configured, and then
    /// navigates the RunPageModel into view for the user. This allows them to finish configuring the run and then run it.
    /// </summary>
    private async Task NavigateRunPage()
    {
        var run = RunObserver.Virtual(this, out var node);
        await Navigator.Navigate(() => new RunPageModel(node, run));
    }

    /// <summary>
    /// If this is the run node type, then we simply load up the configured run and navigate it into the RunnerPageModel
    /// which should then kick off the execution of the run.
    /// </summary>
    private async Task NavigateRunnerPage()
    {
        var result = await Mediator.Send(new GetRun(Id));

        if (result.IsFailed)
        {
            //notify failure?
            return;
        }

        var run = new RunObserver(result.Value);
        var page = await Navigator.Navigate<RunnerPageModel>();
        page.Run = run;
        //await page.StartCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// A message to be sent when the <see cref="NodeObserver"/> name changes so that other node instance can respond and
    /// update their local value and in turn refresh the UI.
    /// </summary>
    public record Renamed(NodeObserver Node);

    /// <summary>
    /// A message to be sent when the <see cref="NodeObserver"/> is moved to a different parent container. This is so that
    /// other node instances can respond by removing the moved node from its node children.
    /// </summary>
    public record Moved(NodeObserver Node);

    /// <summary>
    /// A request message that will return all selected <see cref="NodeObserver"/> instance to the requesting node. 
    /// </summary>
    public class GetSelected : CollectionRequestMessage<NodeObserver>;
}