using System;
using MediatR;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;

namespace AutoSpex.Client.ViewModels;

public partial class CollectionViewModel : NodeDetailViewModel
{
    private readonly IMediator _mediator;
    
    public CollectionViewModel(NodeObserver node) : base(node)
    {
        _mediator = Container.Resolve<IMediator>();
    }

    protected override Task Load()
    {
        return base.Load();
    }

    protected override Task Save()
    {
        throw new NotImplementedException();
    }

    protected override bool CanSave()
    {
        throw new NotImplementedException();
    }
}