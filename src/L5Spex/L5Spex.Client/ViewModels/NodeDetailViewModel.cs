using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using L5Spex.Client.Observers;
using MediatR;

namespace L5Spex.Client.ViewModels;

public partial class NodeDetailViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    
    [ObservableProperty] private ObservableCollection<NodeObserver> _nodes = new();

    [ObservableProperty] private NodeObserver _selectedNode;
    
    public NodeDetailViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }
}