using System;
using System.Collections.Generic;
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
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Get<NodeObserver>>,
    IRecipient<Observer.Find<NodeObserver>>,
    IRecipient<NodeObserver.Moved>,
    IRecipient<NodeObserver.ExpandTo>
{
    public NodeObserver(Node node) : base(node)
    {
        Nodes = new ObserverCollection<Node, NodeObserver>(
            refresh: () => Model.Nodes.Select(n => new NodeObserver(n)).ToList(),
            add: (_, n) => Model.AddNode(n),
            remove: (_, n) => Model.RemoveNode(n),
            clear: () => Model.ClearNodes(),
            count: () => Model.Nodes.Count());
        
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        Track(Nodes, false);
    }

    public override Guid Id => Model.NodeId;
    public override string Icon => Type.Name;
    public Guid ParentId => Model.ParentId;
    public NodeObserver? Parent => Model.Parent is not null ? new NodeObserver(Model.Parent) : default;
    public NodeType Type => Model.Type;
    public bool IsVirtual => Type != NodeType.Collection && ParentId == Guid.Empty;

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

    public bool FilterSingle(string? filter)
    {
        FilterText = filter;
        return string.IsNullOrEmpty(filter) || Name.Satisfies(filter);
    }

    #region Commands

    /// <summary>
    /// Adds a container node to the collection of nodes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [RelayCommand]
    private Task AddContainer() => AddNode(new NodeObserver(Node.NewContainer()) { IsNew = true });

    /// <summary>
    /// Adds a new spec node to the list of child nodes.
    /// </summary>
    /// <returns>
    /// A Task representing the asynchronous operation.
    /// </returns>
    [RelayCommand]
    private Task AddSpec() => AddNode(new NodeObserver(Node.NewSpec()) { IsNew = true });

    /// <summary>
    /// Command to move the provided node and the selected items to this node container.
    /// </summary>
    /// <param name="source">The node that is to be moved to this node.</param>
    [RelayCommand(CanExecute = nameof(CanMove))]
    private async Task Move(object? source)
    {
        if (source is not NodeObserver node) return;

        //We want to get the selected items from this nodes container to ensure we move all selected nodes.
        var selected = node.SelectedItems.Where(o => o is NodeObserver).Cast<NodeObserver>().ToList();
        if (selected.Any(n => !Type.CanContain(n.Type) || Id == n.Id)) return;

        //Update the database parent id for the selected node.
        var result = await Mediator.Send(new MoveNodes(selected.Select(n => n.Model), Id));
        if (Notifier.ShowIfFailed(result, $"Failed to move selected node to parent {Name}")) return;

        foreach (var moved in selected)
        {
            //Add the selected nodes to the parent to update the parent id correctly.
            Nodes.TryAdd(node);
            //Sends the message to other nodes to remove the moved nodes from their child nodes collection.
            Messenger.Send(new Moved(moved));
        }
    }

    /// <summary>
    /// Determines if the provided source object is a node which has selected nodes that can be moved to this node.
    /// </summary>
    /// <param name="source">The source command parameter.</param>
    /// <returns>true if the provided source contains valid nodes to be added to this node. Otherwise, false.</returns>
    private bool CanMove(object? source)
    {
        if (source is not NodeObserver node) return false;
        if (Type == NodeType.Spec) return false;
        if (node.Id == Id) return false;
        var selected = node.SelectedItems.Where(o => o is NodeObserver).Cast<NodeObserver>().ToList();
        return selected.All(n => Type.CanContain(n.Type) && Id != n.Id);
    }

    /// <summary>
    /// Command to allow the user to select a new node to move the selected nodes to.
    /// </summary>
    [RelayCommand]
    private async Task MoveTo()
    {
        //Prompt the user to select the node instance to move the selected nodes to.
        var parent = await Prompter.Show<NodeObserver?>(() => new SaveToContainerPageModel());
        if (parent is null) return;

        //We want to get the selected items from this nodes container to ensure we move all selected nodes.
        var selected = SelectedItems.Where(o => o is NodeObserver).Cast<NodeObserver>().ToList();
        if (selected.Any(n => !parent.Type.CanContain(n.Type) || parent.Id == n.Id)) return;

        //Update the database parent id for the selected node.
        var result = await Mediator.Send(new MoveNodes(selected.Select(n => n.Model), parent.Id));
        if (Notifier.ShowIfFailed(result, $"Failed to move selected node to parent {parent.Name}")) return;

        foreach (var node in selected)
        {
            //Add the selected nodes to the parent to update the parent id correctly.
            parent.Nodes.TryAdd(node);
            //Sends the message to other nodes to remove the moved nodes from their child nodes collection.
            Messenger.Send(new Moved(node));
        }
    }

    /// <summary>
    /// Command to run only selected specs from the parent container.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This command will allow the user to select which specs or nodes to run instead of running all descendants.
    /// </remarks>
    [RelayCommand]
    private async Task Run()
    {
        //We need to load the full environment to get sources and overrides.
        var loadEnvironment = await Mediator.Send(new LoadTargetEnvironment());
        if (Notifier.ShowIfFailed(loadEnvironment, "Failed to load the target environment.")) return;

        //Get selected node from the same container as this node.
        var selected = SelectedItems.Where(x => x is NodeObserver).Cast<NodeObserver>().Select(n => n.Model);

        //Build the run object.
        var run = new Run(loadEnvironment.Value);
        run.AddNodes(selected);

        //Navigate the run page model which will then trigger the run process
        await Navigator.Navigate(() => new RunDetailPageModel(run));
    }

    /// <inheritdoc />
    /// <remarks>
    /// For a node we wil prompt the user for a new name, then duplicate the entire node instance, including
    /// all variables, specs, and child nodes.
    /// </remarks>
    protected override async Task Duplicate()
    {
        var name = await Prompter.PromptNewName(this);
        if (name is null) return;

        var load = await Mediator.Send(new LoadNode(Id));
        if (Notifier.ShowIfFailed(load)) return;

        var duplicate = load.Value.Duplicate(name);

        var result = await Mediator.Send(new CreateNode(duplicate));
        if (Notifier.ShowIfFailed(result)) return;

        Messenger.Send(new Created(new NodeObserver(duplicate)));
        Notifier.ShowSuccess(
            "Create node request complete",
            $"{duplicate.Name} was successfully created @ {DateTime.Now}"
        );
    }

    /// <summary>
    /// A command to export the current node content to a package that the user can then transfer to different apps. 
    /// </summary>
    [RelayCommand]
    private async Task Export()
    {
        if (Type != NodeType.Collection) return;

        try
        {
            var export = await Mediator.Send(new ExportNode(Id));
            if (export.IsFailed)
            {
                Notifier.NofityExportFailed(Name, export.Errors.Select(e => e.Message));
                return;
            }

            var result = await Shell.StorageProvider.ExportPackage(export.Value);
            if (result.IsFailed)
            {
                Notifier.NofityExportFailed(Name, export.Errors.Select(e => e.Message));
            }
        }
        catch (Exception e)
        {
            Notifier.ShowError("Export failed.", $"Failed to process export due to {e.Message}");
        }
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

    #endregion

    #region Handlers

    /// <summary>
    /// Will handle the creation of a node. Parent nodes need to ensure the object is added if it belongs as a child.
    /// Also, this is where we want to sort the children to ensure proper ordering. Also trigger the locate command
    /// to allow the UI to select and display this node in the tree.
    /// </summary>
    public void Receive(Created message)
    {
        if (message.Observer is not NodeObserver node) return;

        if (Id != node.ParentId) return;
        if (Nodes.Any(n => n.Id == node.Id)) return;

        Nodes.Add(new NodeObserver(node));
        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        node.Locate();
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
    /// Handles the observer moved message by removing from the old parent if the recieved
    /// node is a child of the old or new node.
    /// </summary>
    public void Receive(Moved message)
    {
        if (!Nodes.Contains(message.Node)) return;
        if (message.Node.ParentId == Id) return;
        Nodes.Remove(message.Node);
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
    public void Receive(Get<NodeObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate.Invoke(this))
        {
            message.Reply(this);
        }
    }

    /// <summary>
    /// Respond the to find request message for a node observer instance by checking the provided predicate.
    /// </summary>
    public void Receive(Find<NodeObserver> message)
    {
        if (message.Predicate.Invoke(this) && message.Responses.All(x => x.Id != Id))
        {
            message.Reply(this);
        }
    }

    #endregion

    #region Messages

    /// <summary>
    /// A message to be sent when nodes are moved to a different parent container. This is so that
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

        //If this node has not been saved to the database, just return ok to allow the node to be renamed.
        //Otherwise, send the request to update the database.
        return IsVirtual ? Task.FromResult(Result.Ok()) : Mediator.Send(new RenameNode(this));
    }

    protected override Task<Result> DeleteItems(IEnumerable<Observer> observers)
    {
        //If this node has not been saved to the database, just return ok to allow the node to be deleted virtually.
        //Otherwise, send the request to update the database.
        return IsVirtual ? Task.FromResult(Result.Ok()) : Mediator.Send(new DeleteNodes(observers.Select(n => n.Id)));
    }

    private async Task AddNode(NodeObserver node)
    {
        //Add before sending request because this sets the parent correctly.
        Nodes.Add(node);

        var result = await Mediator.Send(new CreateNode(node));
        if (Notifier.ShowIfFailed(result, $"Failed to create new {node.Type} {node.Name}."))
        {
            Nodes.Remove(node);
            return;
        }

        Nodes.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
        Locate();
        Messenger.Send(new Created(node));
        await Navigator.Navigate(node);
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Add Container",
            Icon = Resource.Find("IconThemedContainer"),
            Command = AddContainerCommand,
            DetermineVisibility = () => HasSingleSelection && Type != NodeType.Spec
        };

        yield return new MenuActionItem
        {
            Header = "Add Spec",
            Icon = Resource.Find("IconThemedSpec"),
            Command = AddSpecCommand,
            DetermineVisibility = () => HasSingleSelection && Type != NodeType.Spec
        };

        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent",
            Command = RunCommand
        };

        yield return new MenuActionItem
        {
            Header = "Open",
            Icon = Resource.Find("IconLineLaunch"),
            Command = NavigateCommand,
            Gesture = new KeyGesture(Key.Enter),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Export",
            Icon = Resource.Find("IconLineDownload"),
            Command = ExportCommand,
            DetermineVisibility = () => HasSingleSelection && IsSelected && Type == NodeType.Collection
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPencil"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Move",
            Icon = Resource.Find("IconLineMove"),
            Command = MoveToCommand,
            DetermineVisibility = () => Type != NodeType.Collection
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteSelectedCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;
}