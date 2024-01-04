using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

public abstract partial class NodeDetailViewModel : ViewModelBase
{
    protected NodeDetailViewModel(Node node)
    {
        Node = node;
    }

    [ObservableProperty] private Node _node;

    [ObservableProperty] private bool _inFocus;
    
    [RelayCommand]
    private void Navigate(Breadcrumb breadcrumb)
    {
        Messenger.Send(new OpenNodeMessage(breadcrumb.Node));
    }

    [RelayCommand]
    private async Task Rename(Breadcrumb breadcrumb)
    {
        await Mediator.Send(new RenameNodeRequest(breadcrumb.Node));
    }

    public override bool Equals(object? obj) => obj is NodeDetailViewModel other && Node.NodeId == other.Node.NodeId;

    public override int GetHashCode() => Node.NodeId.GetHashCode();
}