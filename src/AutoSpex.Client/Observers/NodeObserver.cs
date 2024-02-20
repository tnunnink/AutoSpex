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
using CommunityToolkit.Mvvm.Messaging.Messages;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[PublicAPI]
public partial class NodeObserver : Observer<Node>,
    IRecipient<NodeObserver.Deleted>,
    IRecipient<PropertyChangedMessage<string>>
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

        Messenger.Register<PropertyChangedMessage<string>, Guid>(this, NodeId);
        Messenger.Register<Deleted, Guid>(this, NodeId);

        IsOrphaned = Model.NodeType != NodeType.Collection && Model.ParentId == Guid.Empty;
    }

    public Guid NodeId => Model.NodeId;
    public NodeType NodeType => Model.NodeType;

    [ObservableProperty] [NotifyDataErrorInfo] [Required] [MaxLength(100)]
    private string _name;

    public ObserverCollection<Node, NodeObserver> Nodes { get; }

    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private bool _isEditing;

    [ObservableProperty] private bool _isOrphaned;

    [ObservableProperty] private bool _focusName;

    /// <summary>
    /// We are overriding the refresh here to sync the underlying model state which includes the name property for this
    /// node and all child nodes.
    /// </summary>
    public override void Refresh()
    {
        Name = Model.Name;
        Nodes.Refresh();
        base.Refresh();
    }

    [RelayCommand]
    private async Task AddFolder()
    {
        var node = new NodeObserver(Node.NewFolder()) {FocusName = true};
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
        var node = new NodeObserver(Node.NewSpec()) {FocusName = true};
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
    /// This command syncs the current observer name property to the underlying model object property. It then sends the
    /// <see cref="PropertyChangedMessage{T}"/> for the name property so other observer objects or pages can react to the
    /// change. Note that if this is an "orphaned" node we assume there is not record in the database so we just update
    /// this object instance and don't send the command to the database.
    /// </summary>
    [RelayCommand]
    private async Task RenameNode()
    {
        if (string.IsNullOrEmpty(Name) || string.Equals(Name, Model.Name) || Name.Length > 100) return;

        var previous = Model.Name;
        Model.Name = Name;

        if (IsOrphaned)
        {
            Messenger.Send(new PropertyChangedMessage<string>(this, nameof(Name), previous, Model.Name), NodeId);
            return;
        }

        var result = await Mediator.Send(new RenameNode(this));

        if (result.IsFailed)
        {
            Model.Name = previous;
            Name = previous;
            return;
        }

        Messenger.Send(new PropertyChangedMessage<string>(this, nameof(Name), previous, Model.Name), NodeId);
    }

    /// <summary>
    /// The reception of the node name change message will allow us to propagate the change to other observers that wrap
    /// the same object but are of a different memory instance. We still want to be able to update these other instance
    /// and have them in turn trigger the property changed so containing pages (like NodePageModel) can receive and update
    /// the UI accordingly. We just have to be careful here not to cause a stack overflow so we exit early on all the
    /// conditions for which we would not want to set the name and send the message.
    /// </summary>
    /// <param name="message">The <see cref="PropertyChangedMessage{T}"/> contianing the information regarding the property change.</param>
    public void Receive(PropertyChangedMessage<string> message)
    {
        if (message.Sender is not NodeObserver node || message.PropertyName != nameof(Name)) return;
        if (ReferenceEquals(this, node)) return;
        if (node.NodeId != NodeId) return;
        if (Name == message.NewValue || Name != message.OldValue) return;
        Name = message.NewValue;
        Messenger.Send(new PropertyChangedMessage<string>(this, nameof(Name), message.OldValue, Name), NodeId);
    }

    /// <inheritdoc />
    /// <remarks>...</remarks>
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDelete(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteNode(NodeId));
        if (result.IsFailed) return; //todo failed commands will get caught and presented in pipeline.

        var parentId = Model.ParentId;
        Model.Parent?.RemoveNode(this);

        Messenger.Send(new Deleted(this), NodeId); //Delete for this and all children.
        Messenger.Send(new Deleted(this), parentId); //Tell the parent to refresh state including nodes.
        Messenger.Send(new Deleted(this)); //Tell anyone else just listening for deletions.
    }

    public void Receive(Deleted message)
    {
        if (message.Node.NodeId != NodeId)
        {
            Refresh();
            return;
        }

        Nodes.ToList().ForEach(n => Messenger.Send(new Deleted(n), n.NodeId));
        Nodes.Clear();
        Messenger.UnregisterAll(this);
    }

    public static implicit operator NodeObserver(Node node) => new(node);
    public static implicit operator Node(NodeObserver observer) => observer.Model;

    public record Deleted(NodeObserver Node);
}