using System;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoSpex.Client.Pages;

public abstract partial class NodePageModel : DetailPageModel,
    IRecipient<PropertyChangedMessage<string>>,
    IRecipient<NodeObserver.Deleted>
{
    private readonly Guid _nodeId;

    /// <inheritdoc/>
    protected NodePageModel(Node node)
    {
        _nodeId = node.NodeId;

        Node = new NodeObserver(node);
        Breadcrumb = new Breadcrumb(Node, CrumbType.Target);
        Variables = new ObserverCollection<Variable, VariableObserver>(
            () => Node.Model.ScopedVariables().Select(v => new VariableObserver(v)).ToList(),
            (_, v) => Node.Model.AddVariable(v),
            (_, v) => Node.Model.AddVariable(v),
            (_, v) => Node.Model.RemoveVariable(v));
        Track(Variables);
    }

    public override string Route => $"{GetType().Name}/{_nodeId}";
    public override string Title => Node.Name;
    public override string Icon => Node.NodeType.Name;

    [ObservableProperty] private NodeObserver _node;

    [ObservableProperty] private Breadcrumb _breadcrumb;

    [ObservableProperty] private bool _notFound;

    public ObserverCollection<Variable, VariableObserver> Variables { get; }

    protected override void OnActivated()
    {
        Messenger.Register<PropertyChangedMessage<string>, Guid>(this, _nodeId);
        Messenger.Register<NodeObserver.Deleted, Guid>(this, _nodeId);
    }

    public override async Task Load()
    {
        //This is telling the UI to capture focus to the name of the node for immediate editing.
        if (Node.FocusName)
        {
            Dispatcher.UIThread.Post(() => Breadcrumb.InFocus = true);
        }

        //An orphaned node has not been persisted yet.
        if (!Node.IsOrphaned)
        {
            await LoadFullNode();
        }

        SaveCommand.NotifyCanExecuteChanged();

        //await Navigator.Navigate(() => new NodeInfoPageModel(Node));
    }

    private async Task LoadFullNode()
    {
        var result = await Mediator.Send(new GetFullNode(_nodeId));

        if (result.IsFailed)
        {
            NotFound = true;
        }

        Node = new NodeObserver(result.Value);
    }

    protected override async Task Save()
    {
        if (Node.IsOrphaned)
        {
            //todo then we need to open the "save as/to" popup the should probably just add the node whatever parent they select which will set the parent id.
            return;
        }

        var result = await Mediator.Send(new SaveNode(Node));

        if (!result.IsSuccess) return;

        var notification = new Notification($"{Node.Name} Saved",
            $"Saved {Node.Name} successfully @ {DateTime.Now.ToShortTimeString()}"
            , NotificationType.Success);
        Notifier.Notify(notification);

        AcceptChanges();
    }

    protected override bool CanSave() => IsChanged || Node.IsOrphaned;

    public void Receive(NodeObserver.Deleted message)
    {
        if (message.Node.NodeId != _nodeId) return;
        ForceClose();
    }

    public void Receive(PropertyChangedMessage<string> message)
    {
        if (message is {Sender: NodeObserver node, PropertyName: nameof(NodeObserver.Name)} && node.NodeId == _nodeId)
        {
            OnPropertyChanged(nameof(Title));
        }
    }

    public override bool Equals(object? obj) => obj is NodePageModel other && _nodeId == other._nodeId;
    public override int GetHashCode() => _nodeId.GetHashCode();
}