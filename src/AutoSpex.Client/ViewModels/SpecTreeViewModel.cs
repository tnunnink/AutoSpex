using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;

namespace AutoSpex.Client.ViewModels;

[UsedImplicitly]
public partial class SpecTreeViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<NodeObserver> _nodes = new();

    [ObservableProperty] private ObservableCollection<NodeObserver> _selectedNodes = new();

    [ObservableProperty] private NodeObserver? _selectedNode;

    [ObservableProperty] private string _filter = string.Empty;

    [RelayCommand]
    private async Task GetNodes()
    {
        var result = await Mediator.Send(new GetNodes(Feature.Specifications));

        if (result.IsSuccess)
        {
            Nodes = new ObservableCollection<NodeObserver>(result.Value.Select(n => (NodeObserver) n));
        }
    }

    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.Collection(Feature.Specifications, "New Collection");

        var result = await Mediator.Send(new AddNode(node));

        if (result.IsSuccess)
        {
            Nodes.Add(result.Value);
            SelectedNode = node;
            OpenInFocus(node);
        }
    }

    [RelayCommand]
    private async Task AddFolder(NodeObserver parent)
    {
        var node = parent.Model.NewFolder();

        var result = await Mediator.Send(new AddNode(node));

        if (result.IsSuccess)
        {
            //parent.AddNode(node);
            SelectedNode = node;
            OpenInFocus(node);
        }
    }

    [RelayCommand]
    private async Task AddSpecification(NodeObserver parent)
    {
        var node = parent.Model.NewSpec();

        var result = await Mediator.Send(new AddNode(node));

        if (result.IsSuccess)
        {
            //parent.AddNode(node);
            SelectedNode = node;
            OpenInFocus(node);
        }
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
            Messenger.Send(new NodeRenamedMessage(node));
        }
    }

    [RelayCommand]
    private async Task DeleteNode(NodeObserver? node)
    {
        if (node is null) return;

        //todo we need to issue a confirmation dialog first. should we do that in the request?

        var result = await Mediator.Send(new DeleteNodeRequest(node.Model.NodeId));

        if (result.IsSuccess)
        {
            if (node.Model.Parent is null)
            {
                Nodes.Remove(node);
                return;
            }

            //node.Model.Parent.Nodes.Remove(node);
        }
    }

    [RelayCommand]
    private void Open(NodeObserver? node)
    {
        if (node is null) return;

        var message = new OpenNodeMessage(node);

        Messenger.Send(message);
    }

    [RelayCommand]
    private void OpenInTab(NodeObserver? node)
    {
        if (node is null) return;

        var message = new OpenNodeMessage(node) {InTab = true};

        Messenger.Send(message);
    }

    private void OpenInFocus(NodeObserver? node)
    {
        if (node is null) return;

        var message = new OpenNodeMessage(node) {InFocus = true};

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