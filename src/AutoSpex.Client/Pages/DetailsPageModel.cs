using System;
using System.Collections.ObjectModel;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Sources;
using AutoSpex.Client.Features.Specifications;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;
using NodeViewModel = AutoSpex.Client.Features.Nodes.NodeViewModel;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class DetailsPageModel : ViewModelBase, IRecipient<OpenNode>
{
    [ObservableProperty] private ObservableCollection<NodeViewModel> _nodes = new();

    [ObservableProperty] private NodeViewModel? _selected;

    public DetailsPageModel()
    {
        Nodes.Add(new SourceViewModel(new Node("source name", NodeType.Source)));
        Nodes.Add(new SourceViewModel(new Node("another name", NodeType.Source)));
    }

    public DetailsPageModel(IMediator mediator, IMessenger messenger)
    {
        /*Nodes.Add(new SourceViewModel(new Node("source name", NodeType.Source)));
        Nodes.Add(new SourceViewModel(new Node("another name", NodeType.Source)));*/
        
        messenger.RegisterAll(this);
    }

    public void Receive(OpenNode message)
    {
        switch (message.Node.NodeType)
        {
            case NodeType.Collection:
                var projectDetails = new SpecificationViewModel(message.Node);
                AddOrSetNode(projectDetails);
                break;
            case NodeType.Folder:
                var folderDetails = new SpecificationViewModel(message.Node);
                AddOrSetNode(folderDetails);
                break;
            case NodeType.Spec:
                var specificationDetails = new SpecificationViewModel(message.Node);
                AddOrSetNode(specificationDetails);
                break;
            case NodeType.Source:
                var sourceViewModel = new SourceViewModel(message.Node);
                AddOrSetNode(sourceViewModel);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message),
                    $"The provided node type {message.Node.NodeType} is not supported by the details view model.");
        }
    }

    [RelayCommand]
    private void CloseNode(NodeViewModel node)
    {
        if (node.IsChanged)
        {
            //todo prompt user.
        }

        Nodes.Remove(node);
    }

    private void AddOrSetNode(NodeViewModel nodes)
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