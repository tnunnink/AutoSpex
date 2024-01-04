using System.Collections.ObjectModel;
using AutoSpex.Client.Features.Collections;
using AutoSpex.Client.Features.Folders;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Specifications;

[UsedImplicitly]
public partial class SpecTreeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;

    public SpecTreeViewModel(IMediator mediator, IMessenger messenger)
    {
        _mediator = mediator;
        _messenger = messenger;
    }

    [ObservableProperty] private ObservableCollection<Node> _nodes = new();

    [ObservableProperty] private ObservableCollection<Node> _selectedNodes = new();

    [ObservableProperty] private Node? _selectedNode = new();

    [ObservableProperty] private string _filter = string.Empty;

    [RelayCommand]
    private async Task GetNodes()
    {
        var result = await _mediator.Send(new GetNodesRequest(Feature.Specifications));

        if (result.IsSuccess)
        {
            Nodes = new ObservableCollection<Node>(result.Value);
        }
    }

    [RelayCommand]
    private async Task AddCollection()
    {
        var node = Node.SpecCollection("New Collection");

        var result = await _mediator.Send(new AddCollectionRequest(node));

        if (result.IsSuccess)
        {
            Nodes.Add(result.Value);
            SelectedNode = node;
            OpenInFocus(node);
        }
    }

    [RelayCommand]
    private async Task AddFolder(Node parent)
    {
        var node = parent.NewFolder();

        var result = await _mediator.Send(new AddFolderRequest(node));

        if (result.IsSuccess)
        {
            parent.AddNode(node);
            SelectedNode = node;
            OpenInFocus(node);
        }
    }

    [RelayCommand]
    private async Task AddSpecification(Node parent)
    {
        var node = parent.NewSpec();

        var result = await _mediator.Send(new AddNodeRequest(node));

        if (result.IsSuccess)
        {
            parent.AddNode(node);
            SelectedNode = node;
            OpenInFocus(node);
        }
    }

    [RelayCommand]
    private async Task RenameNode(Node? node)
    {
        if (node is null) return;

        var name = string.Empty;

        if (string.IsNullOrEmpty(name)) return;

        node.Name = name;

        var result = await _mediator.Send(new RenameNodeRequest(node));

        if (result.IsSuccess)
        {
            _messenger.Send(new NodeRenamedMessage(node));
        }
    }

    [RelayCommand]
    private async Task DeleteNode(Node? node)
    {
        if (node is null) return;

        //todo we need to issue a confirmation dialog first. should we do that in the request?

        var result = await _mediator.Send(new DeleteNodeRequest(node.NodeId));

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
    private void Open(Node? node)
    {
        if (node is null) return;

        var message = new OpenNodeMessage(node);

        _messenger.Send(message);
    }

    [RelayCommand]
    private void OpenInTab(Node? node)
    {
        if (node is null) return;

        var message = new OpenNodeMessage(node) {InTab = true};

        _messenger.Send(message);
    }

    private void OpenInFocus(Node? node)
    {
        if (node is null) return;

        var message = new OpenNodeMessage(node) {InFocus = true};

        _messenger.Send(message);
    }

    /*partial void OnFilterTextChanged(string value)
    {
        foreach (var node in Collections)
        {
            node.FilterPath(value);
        }
    }*/
}