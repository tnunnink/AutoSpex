using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[PublicAPI]
public partial class NodeObserver : Observer<Node>,
    IRecipient<Observer<Node>.Created>,
    IRecipient<Observer<Node>.Deleted>,
    IRecipient<Observer<Node>.Renamed>,
    IRecipient<VariableObserver.NodeRequest>
{
    public NodeObserver(Node node) : base(node)
    {
        Name = node.Name;

        Nodes = new ObserverCollection<Node, NodeObserver>(
            () => Model.Nodes.Select(n => new NodeObserver(n)).ToList(),
            (_, n) => Model.AddNode(n),
            (i, n) => Model.InsertNode(n, i),
            (_, n) => Model.RemoveNode(n),
            () => Model.ClearNodes());

        Variables = new ObserverCollection<Variable, VariableObserver>(
            () => Model.Variables.Select(m => new VariableObserver(m)).ToList(),
            (_, m) => Model.AddVariable(m),
            (_, m) => Model.AddVariable(m),
            (_, m) => Model.RemoveVariable(m));
        Track(Variables);

        if (node.Spec is not null)
        {
            Spec = new SpecObserver(node.Spec);
            Track(Spec);
        }

        IsOrphaned = Model.NodeType != NodeType.Collection && Model.ParentId == Guid.Empty;
    }

    public override Guid Id => Model.NodeId;
    public Guid ParentId => Model.ParentId;
    public NodeType NodeType => Model.NodeType;
    public string Path => Model.Path;

    [Required]
    [MaxLength(100)]
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v, true);
    }

    public ObserverCollection<Node, NodeObserver> Nodes { get; }
    public ObserverCollection<Variable, VariableObserver> Variables { get; }

    [ObservableProperty] private SpecObserver? _spec;

    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isEditing;

    [ObservableProperty] private bool _isOrphaned;

    [ObservableProperty] private bool _isNew;

    #region Commands

    [RelayCommand]
    private async Task AddFolder()
    {
        var node = new NodeObserver(Node.NewFolder()) {IsNew = true};
        Nodes.Add(node);

        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsFailed)
        {
            Nodes.Remove(node);
            return;
        }

        IsExpanded = true;
        IsSelected = false;
        node.IsSelected = true;
        await Navigator.Navigate(node);
    }

    /// <summary>
    /// This command creates a new default spec node and adds it to the current node as a child. It will send the command
    /// to the database to create the new node and upon success we will navigate to this node in the tree and details
    /// by expanding, selecting, and sending the navigation request.
    /// </summary>
    [RelayCommand]
    private async Task AddSpec()
    {
        var node = new NodeObserver(Node.NewSpec()) {IsNew = true};
        Nodes.Add(node);

        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsFailed)
        {
            Nodes.Remove(node);
            return;
        }

        IsExpanded = true;
        IsSelected = false;
        node.IsSelected = true;
        await Navigator.Navigate(node);
    }

    /// <inheritdoc />
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDelete(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteNode(Id));
        if (result.IsFailed) return;

        //We don't have access to the actual parent observer only the underlying model object. Therefore we will rely
        //on messages send to be handles by the parent and remove this node form it's Nodes collection.
        Messenger.Send(new Deleted(this));
    }

    /// <inheritdoc />
    protected override async Task Rename(string? name)
    {
        if (string.IsNullOrEmpty(name) || string.Equals(name, Name) || name.Length > 100) return;

        var previous = Name;
        Name = name;

        if (IsOrphaned)
        {
            Messenger.Send(new Renamed(this));
            return;
        }

        var result = await Mediator.Send(new RenameNode(this));

        if (result.IsFailed)
        {
            Name = previous;
            return;
        }

        Messenger.Send(new Renamed(this));
    }

    [RelayCommand]
    private void Edit()
    {
        IsEditing = true;
    }

    #endregion

    #region MessageHandlers

    public void Receive(Created message)
    {
        if (message.Observer is not NodeObserver node) return;

        //If I am the parent of the created node then make sure it is added to my Nodes
        if (Id == node.ParentId && Nodes.All(n => n.Id != node.Id))
        {
            // This will also add to the underlying model object which is fine because
            // we assume the node was added to a different model somewhere.
            Nodes.Add(node);
        }
    }

    /// <summary>
    /// Will handle the removal of the node and/or child nodes as based on which observer object is received.
    /// </summary>
    /// <param name="message">The message that an observer has been deleted.</param>
    public void Receive(Deleted message)
    {
        if (message.Observer is not NodeObserver node) return;

        //If the deleted node is my child then remove if from nodes and return.
        //This will in turn remove it from the model object as well.
        if (Nodes.Any(n => n.Id == node.Id))
        {
            Nodes.Remove(node);
            return;
        }

        //If the deleted node is me then send messages for all the child nodes and clear the collection.
        //We need to send messages for children in case they are in use by other pages.
        //Doing in the message handler will all it to be propagated.
        if (Id != node.Id) return;
        Nodes.ToList().ForEach(n => Messenger.Send(new Deleted(n)));
        Messenger.UnregisterAll(this);
    }

    public void Receive(Renamed message)
    {
        if (message.Observer is not NodeObserver node) return;
        if (ReferenceEquals(this, node)) return;
        if (Id != node.Id) return;

        if (Name != node.Name)
        {
            Name = node.Name;
            Messenger.Send(new Renamed(this));
            return;
        }

        OnPropertyChanged(nameof(Name));
    }

    public void Receive(VariableObserver.NodeRequest message)
    {
        if (message.HasReceivedResponse) return;

        if (Variables.Any(v => v.Id == message.VariableId))
        {
            message.Reply(this);
        }
    }

    #endregion

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;
}