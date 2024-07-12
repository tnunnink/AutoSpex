using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[PublicAPI]
public partial class NodeObserver : Observer<Node>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Request<NodeObserver>>,
    IRecipient<NodeObserver.Moved>,
    IRecipient<NodeObserver.ExpandTo>
{
    public NodeObserver(Node node) : base(node)
    {
        Nodes = new ObserverCollection<Node, NodeObserver>(
            () => Model.Nodes.Select(n => new NodeObserver(n)).ToList(),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.AddNode(n),
            (_, n) => Model.RemoveNode(n),
            () => Model.ClearNodes());
    }

    public override Guid Id => Model.NodeId;
    public override string Icon => Type.Name;
    public Guid ParentId => Model.ParentId;
    public NodeObserver? Parent => Model.Parent is not null ? new NodeObserver(Model.Parent) : default;
    public NodeType Type => Model.Type;

    [Required]
    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v, true);
    }

    public ObserverCollection<Node, NodeObserver> Nodes { get; }
    public IEnumerable<NodeObserver> Crumbs => Model.Ancestors().Select(n => new NodeObserver(n));
    public IEnumerable<NodeObserver> Path => Model.AncestorsAndSelf().Select(n => new NodeObserver(n));

    /// <inheritdoc />
    /// <remarks>
    /// Since the node is a tree we want to also filter depth first to find children containing the text.
    /// If found we set the visibility and expand up to the found items.
    /// </remarks>
    public override bool Filter(string? filter)
    {
        var passes = base.Filter(filter);
        var children = Nodes.Count(x => x.Filter(filter));

        IsVisible = passes || children > 0;
        IsExpanded = children > 0;

        return IsVisible;
    }

    /// <summary>
    /// Traverses the entire node tree to determine if it contains the provided node observer instance.
    /// </summary>
    /// <param name="observer">The node instance to find.</param>
    /// <returns><c>true</c> if the node is in the tree, otherwise, <c>false</c>.</returns>
    public bool HasNode(NodeObserver observer)
    {
        foreach (var node in Nodes)
        {
            if (node.Is(observer)) return true;
            if (node.HasNode(observer)) return true;
        }

        return false;
    }

    #region Commands

    [RelayCommand]
    private Task AddContainer() => AddNode(new NodeObserver(Node.NewContainer()) { IsNew = true });

    [RelayCommand]
    private Task AddSpec() => AddNode(new NodeObserver(Node.NewSpec()) { IsNew = true });

    /// <inheritdoc />
    protected override async Task Delete()
    {
        if (!IsSelected) return;

        var selected = SelectedItems.ToList();

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
    private async Task Move(IList<NodeObserver> nodes)
    {
        if (nodes.Any(n => !Type.CanContain(n.Type))) return;

        var result = await Mediator.Send(new MoveNodes(nodes.Select(n => n.Model), Id));
        if (result.IsFailed) return;

        Nodes.AddRange(nodes);

        foreach (var node in nodes)
            Messenger.Send(new Moved(node));
    }

    [RelayCommand]
    private async Task MoveSelected()
    {
        var selected = SelectedItems.Where(x => x is NodeObserver).Cast<NodeObserver>().ToList();

        var parent = await Prompter.Show<NodeObserver?>(() => new SelectContainerPageModel());
        if (parent is null) return;

        if (selected.Any(n => !parent.Type.CanContain(n.Type))) return;

        var result = await Mediator.Send(new MoveNodes(selected.Select(n => n.Model), parent.Id));
        if (result.IsFailed) return;

        parent.Nodes.AddRange(selected);

        foreach (var node in selected)
            Messenger.Send(new Moved(node));
    }

    /// <summary>
    /// Sends the command to expand the tree and select this node, thereby locating this node in the hierarchy.
    /// </summary>
    [RelayCommand]
    public void Locate()
    {
        Messenger.Send(new ExpandTo(this));
        IsSelected = true;
    }

    /// <summary>
    /// Sends the command to expand the tree and select this node, thereby locating this node in the hierarchy.
    /// </summary>
    [RelayCommand]
    public void ExpandAll()
    {
        IsExpanded = true;

        foreach (var node in Nodes)
        {
            node.ExpandAll();
        }
    }

    /// <summary>
    /// Sends the command to expand the tree and select this node, thereby locating this node in the hierarchy.
    /// </summary>
    [RelayCommand]
    public void CollapseAll()
    {
        IsExpanded = false;

        foreach (var node in Nodes)
        {
            node.CollapseAll();
        }
    }

    /// <summary>
    /// A command to run this node and all child specifications.
    /// </summary>
    [RelayCommand]
    private async Task Run()
    {
        //We need to load the full environment to get sources and overrides.
        var result = await Mediator.Send(new GetTargetEnvironment());
        if (result.IsFailed) return;
        var environment = new EnvironmentObserver(result.Value);

        //Create new run instance with the target environment and current node.
        var run = new Run(environment, this);

        await Navigator.Navigate(() => new RunDetailPageModel(run));
    }

    #endregion

    #region Handlers

/*/// <summary>
/// Will handle the creation of a node. Parent nodes need to ensure the object is added if it belongs as a child.
/// Also, this is where we want to sort the children to ensure proper ordering. Also trigger the locate command
/// to allow the UI to select and display this node in the tree.
/// </summary>
public void Receive(Created message)
{
    if (message.Observer is not NodeObserver node) return;

    if (Id != node.ParentId) return;
    if (Nodes.Any(n => n.Id == node.Id)) return;

    Nodes.Add(node);
    Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    Locate();
}*/

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

    /// <inheritdoc />
    public override void Receive(Renamed message)
    {
        base.Receive(message);

        if (message.Observer is not NodeObserver node) return;

        //If I am the parent to the renamed node then sort my children to ensure proper ordering.
        if (Id != node.ParentId) return;
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
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
    /// When the <see cref="ExpandTo"/> message is received we want to expand the parent node if the received node is
    /// a child of this node. When then in turn send the <see cref="ExpandTo"/> up the tree until we reach the root.
    /// </summary>
    public void Receive(ExpandTo message)
    {
        if (!Nodes.Contains(message.Node)) return;
        IsExpanded = true;
        Messenger.Send(new ExpandTo(this));
    }

    /// <summary>
    /// Respond to the request message of a node observer instance if the id matches this node's id.
    /// </summary>
    public void Receive(Request<NodeObserver> message)
    {
        if (message.HasReceivedResponse) return;
        if (Id != message.Id) return;
        message.Reply(this);
    }

    #endregion

    #region Messages

    /// <summary>
    /// A message to be sent when the <see cref="NodeObserver"/> is moved to a different parent container. This is so that
    /// other node instances can respond by removing the moved node from its node children.
    /// </summary>
    public record Moved(NodeObserver Node);

    /// <summary>
    /// A message that informs the parent nodes to expand their container if the provided node is a child of the
    /// received node. this allows use to expand the tree from a child leaf node.
    /// </summary>
    public record ExpandTo(NodeObserver Node);

    #endregion

    protected override Task<Result> UpdateName(string name)
    {
        Model.Name = name;
        return Mediator.Send(new RenameNode(this));
    }

    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Add Container",
            Icon = Resource.Find("IconThemedContainer"),
            Command = AddContainerCommand
        };

        yield return new MenuActionItem
        {
            Header = "Add Spec",
            Icon = Resource.Find("IconThemedSpec"),
            Command = AddSpecCommand,
        };

        yield return MenuActionItem.Separator;

        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent",
            Command = RunCommand
        };

        yield return MenuActionItem.Separator;

        yield return new MenuActionItem
        {
            Header = "Properties",
            Icon = Resource.Find("IconFilledGear"),
            Command = NavigateCommand,
            Gesture = new KeyGesture(Key.Enter)
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPen"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconLineTrash"),
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

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
        Locate();
        Messenger.Send(new Created(node));
        await Navigator.Navigate(node);
    }

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;
}