using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Specifications;

[UsedImplicitly]
public partial class SpecTreeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private readonly IDialogService _dialog;

    public SpecTreeViewModel(IMediator mediator, IMessenger messenger, IDialogService dialog)
    {
        _mediator = mediator;
        _messenger = messenger;
        _dialog = dialog;
    }

    public Task<ObservableCollection<Node>> Nodes => GetNodes();

    [ObservableProperty] private ObservableCollection<Node> _selectedNodes = new();

    [ObservableProperty] private string _filterText = string.Empty;


    [RelayCommand]
    private async Task AddCollection()
    {
        var name = await _dialog.ShowNodeNameDialog("New Collection", NodeType.Collection);
        if (string.IsNullOrEmpty(name)) return;
        
        var result = await _mediator.Send(new AddCollectionRequest(name));

        if (result.IsSuccess)
        {
            OnPropertyChanged(nameof(Nodes));    
        }
    }

    [RelayCommand]
    private async Task AddFolder(Node parent)
    {
        var name = await _dialog.ShowNodeNameDialog("MyFolder", NodeType.Folder);
        if (string.IsNullOrEmpty(name)) return;
        
        var result = await _mediator.Send(new AddFolderRequest(name, parent));
        
        if (result.IsSuccess)
        {
            parent.Nodes.Add(result.Value);
            OnPropertyChanged(nameof(Nodes));
        }
    }

    [RelayCommand]
    private Task AddSpecification(Node parent)
    {
        throw new NotImplementedException();
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
        
        //todo we need to issue a confirmation dialog first. should we do that in the request?
        
        var result = await _mediator.Send(new DeleteNodeRequest(node.NodeId));

        if (result.IsSuccess && node.Parent is not null)
        {
            node.Parent.Nodes.Remove(node);
            OnPropertyChanged(nameof(Nodes));
        }
    }

    [RelayCommand]
    private void OpenNode(Node? node)
    {
        if (node is not null && node.NodeType == NodeType.Spec)
        {
            _messenger.Send(new OpenNode(node));
        }
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

    /*partial void OnFilterTextChanged(string value)
    {
        foreach (var node in Collections)
        {
            node.FilterPath(value);
        }
    }*/

    private async Task<ObservableCollection<Node>> GetNodes()
    {
        var result = await _mediator.Send(new GetNodesRequest(Feature.Specifications));
        return new ObservableCollection<Node>(result.Value);
    }
}