using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.ViewModels;

public abstract partial class NodeDetailViewModel : ViewModelBase
{
    protected NodeDetailViewModel(Observers.NodeObserver node)
    {
        Node = node;
    }

    [ObservableProperty] private Observers.NodeObserver _node;

    [ObservableProperty] private bool _inFocus;
    
    [RelayCommand]
    private void Navigate(Observers.Breadcrumb breadcrumb)
    {
        Messenger.Send(new OpenNodeMessage(breadcrumb.Node));
    }

    [RelayCommand]
    private async Task Rename(Observers.Breadcrumb breadcrumb)
    {
        await Mediator.Send(new RenameNode(breadcrumb.Node));
    }

    public override bool Equals(object? obj) => obj is NodeDetailViewModel other && Node.Model == other.Node.Model;

    public override int GetHashCode() => Node.Model.GetHashCode();
}