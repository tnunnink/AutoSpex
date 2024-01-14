using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
public partial class NodeTreeViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<NodeObserver> _nodes = [];

    [ObservableProperty] private ObservableCollection<NodeObserver> _selectedNodes = [];

    [ObservableProperty] private NodeObserver? _selectedNode;

    [ObservableProperty] private string _filter = string.Empty;

    [RelayCommand]
    private async Task GetNodes()
    {
        var result = await Mediator.Send(new GetNodes());

        if (result.IsSuccess)
        {
            Nodes = new ObservableCollection<NodeObserver>(result.Value.Select(n => (NodeObserver) n));
        }
    }

    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.NewCollection();

        var result = await Mediator.Send(new CreateNode(node));

        if (result.IsSuccess)
        {
            Nodes.Add(node);
            SelectedNode = node;
            OpenInFocus(node);
        }
    }

    [RelayCommand]
    private async Task AddFolder(NodeObserver parent)
    {
        var folder = new NodeObserver(Node.NewFolder());
        parent.Nodes.Add(folder);

        var result = await Mediator.Send(new CreateNode(folder));

        if (result.IsFailed)
        {
            parent.Nodes.Remove(folder);
            return;
        }

        SelectedNode = folder;
        OpenInFocus(folder);
    }

    [RelayCommand]
    private async Task AddSpec(NodeObserver parent)
    {
        var spec = new NodeObserver(Node.NewSpec());
        parent.Nodes.Add(spec);

        var result = await Mediator.Send(new CreateNode(spec));

        if (result.IsFailed)
        {
            parent.Nodes.Remove(spec);
            return;
        }

        SelectedNode = spec;
        OpenInFocus(spec);
    }

    [RelayCommand]
    private async Task RenameNode(NodeObserver? node)
    {
        if (node is null) return;

        var name = string.Empty;

        if (string.IsNullOrEmpty(name)) return;

        node.Name = name;

        var result = await Mediator.Send(new RenameNode(node));

        if (result.IsSuccess)
        {
            var message = new PropertyChangedMessage<string>(this, nameof(NodeObserver.Name), null, node.Name);
            Messenger.Send(message, node.NodeId);
        }
    }

    [RelayCommand]
    private async Task DeleteNode(NodeObserver? node)
    {
        if (node is null) return;

        //todo we need to issue a confirmation dialog first. should we do that in the request?

        var result = await Mediator.Send(new DeleteNode(node.NodeId));

        if (result.IsSuccess)
        {
            if (node.Parent is null)
            {
                Nodes.Remove(node);
                return;
            }

            node.Parent.Nodes.Remove(node);
        }
    }

    [RelayCommand]
    private void Open(NodeObserver? node)
    {
        if (node is null) return;

        var message = new OpenDetailMessage(node.NewDetail());

        Messenger.Send(message);
    }

    [RelayCommand]
    private void OpenInTab(NodeObserver? node)
    {
        if (node is null) return;

        var message = new OpenDetailMessage(node.NewDetail()) {InNewTab = true};

        Messenger.Send(message);
    }

    private void OpenInFocus(NodeObserver? node)
    {
        if (node is null) return;

        var message = new OpenDetailMessage(node.NewDetail()) {NewItem = true};

        Messenger.Send(message);
    }

    /*partial void OnFilterTextChanged(string value)
    {
        foreach (var node in Collections)
        {
            node.FilterPath(value);
        }
    }*/
}