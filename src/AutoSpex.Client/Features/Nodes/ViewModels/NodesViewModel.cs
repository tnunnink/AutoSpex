using System;
using System.Collections.ObjectModel;
using AutoSpex.Client.Features.Sources;
using AutoSpex.Client.Features.Specifications;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

[UsedImplicitly]
public partial class NodesViewModel : ViewModelBase, IRecipient<OpenNode>
{
    [ObservableProperty] private ObservableCollection<NodeDetailViewModel> _nodes = new();

    [ObservableProperty] private NodeDetailViewModel? _selected;

    public NodesViewModel()
    {
    }

    public NodesViewModel(IMediator mediator, IMessenger messenger)
    {
        /*Nodes.Add(new SourceViewModel(new Node("source name", NodeType.Source)));
        Nodes.Add(new SourceViewModel(new Node("another name", NodeType.Source)));*/
        
        messenger.RegisterAll(this);
    }

    public void Receive(OpenNode message)
    {
        message.Node.NodeType
            .When(NodeType.Collection).Then(() =>
            {
                var projectDetails = new SpecificationViewModel(message.Node);
                AddOrSetNode(projectDetails);
            })
            .When(NodeType.Folder).Then(() =>
            {
                var folderDetails = new SpecificationViewModel(message.Node);
                AddOrSetNode(folderDetails);
            })
            .When(NodeType.Spec).Then(() =>
            {
                var specificationDetails = new SpecificationViewModel(message.Node);
                AddOrSetNode(specificationDetails);
            })
            .When(NodeType.Source).Then(() =>
            {
                var sourceViewModel = new SourceViewModel(message.Node);
                AddOrSetNode(sourceViewModel);
            })
            .Default(() => throw new ArgumentOutOfRangeException(nameof(message),
                $"The provided node type {message.Node.NodeType} is not supported by the details view model."));
    }

    [RelayCommand]
    private void CloseNode(NodeDetailViewModel node)
    {
        if (node.IsChanged)
        {
            //todo prompt user.
        }

        Nodes.Remove(node);
    }

    private void AddOrSetNode(NodeDetailViewModel nodes)
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i].IsChanged) continue;
            Nodes[i] = nodes;
            return;
        }

        Nodes.Add(nodes);
    }
}