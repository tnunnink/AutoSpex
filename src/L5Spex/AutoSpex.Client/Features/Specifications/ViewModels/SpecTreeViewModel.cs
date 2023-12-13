using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Specifications;

[UsedImplicitly]
public partial class SpecTreeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    
    private readonly SourceCache<Node, Guid> _nodeCache;
    private readonly ReadOnlyObservableCollection<Node> _nodes;

    public SpecTreeViewModel(IMediator mediator, IMessenger messenger)
    {
        _mediator = mediator;
        _messenger = messenger;
        Run = Initialize();
        

        /*
        _nodeCache = new SourceCache<Node, Guid>(x => x.NodeId)
            .Connect()
            .TransformToTree(n => n.ParentId);*/
    }

    [ObservableProperty] private ObservableCollection<Node> _collections = new();

    [ObservableProperty] private ObservableCollection<Node> _selectedNodes = new();

    [ObservableProperty] private Node? _selectedNode;
    
    [ObservableProperty] private string _filterText = string.Empty;
    

    [RelayCommand]
    private async Task AddCollection()
    {
        var request = new AddCollectionRequest("New Collection");
        var result = await _mediator.Send(request);
    }

    [RelayCommand]
    private async Task AddFolder(Node parent)
    {
        var request = new AddNodeRequest("New Folder", NodeType.Folder, parent.NodeId);
        var result = await _mediator.Send(request);
    }

    [RelayCommand]
    private async Task AddSpecification(Node parent)
    {
        var request = new AddNodeRequest("New Specification", NodeType.Spec, parent.NodeId);
        var result = await _mediator.Send(request);
    }

    [RelayCommand]
    private static void RenameNode(Node? node)
    {
        if (node is null) return;
        node.IsEditing = true;
    }

    [RelayCommand]
    private async Task DeleteNode(Node? node)
    {
        if (node is null) return;
        //todo we need to issue a confirmation dialog first.
        
        var request = new DeleteNodeRequest(node.NodeId);
        var result = await _mediator.Send(request);

        /*await _mediator.Publish(notification);*/
        
    }

    public void Rename(Node node)
    {
        Run = Task.Run(async () =>
        {
            var request = new RenameNodeRequest(node.NodeId, node.Name);
            var result = await _mediator.Send(request);
            //todo probably send notification or something so others can update or refresh bindings
        });
    }
    
    partial void OnSelectedNodeChanged(Node? value)
    {
        if (value is not null && value.NodeType == NodeType.Spec)
        {
            _messenger.Send(new OpenNode(value));
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        foreach (var node in Collections)
        {
            node.FilterPath(value);
        }
    }

    private async Task Initialize()
    {
        var request = new GetNodesRequest(NodeType.Spec);
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            foreach (var node in result.Value)
            {
                Collections.Add(node);
            }
        }
    }
}