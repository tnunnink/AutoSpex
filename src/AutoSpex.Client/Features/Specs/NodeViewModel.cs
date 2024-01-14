using System;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Features;

public abstract partial class NodeViewModel : DetailViewModel
{
    protected NodeViewModel(NodeObserver node)
    {
        Node = node;
    }

    public override Guid Id => Node.NodeId;
    public override string Label => Node.Name;
    public override string Icon => Node.NodeType.Name;

    [ObservableProperty] private NodeObserver _node;

    [ObservableProperty] private bool _inFocus;
    
    [RelayCommand]
    private void Navigate(Breadcrumb breadcrumb)
    {
        Messenger.Send(new OpenDetailMessage(breadcrumb.Node.NewDetail()));
    }

    [RelayCommand]
    private async Task Rename(Breadcrumb breadcrumb)
    {
        await Mediator.Send(new RenameNode(breadcrumb.Node));
    }

    public override bool Equals(object? obj) => obj is NodeViewModel other && Node.Model == other.Node.Model;

    public override int GetHashCode() => Node.Model.GetHashCode();
}