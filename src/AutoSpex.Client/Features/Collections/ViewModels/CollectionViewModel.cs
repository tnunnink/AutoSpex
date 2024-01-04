using System;
using AutoSpex.Client.Features.Nodes;
using MediatR;

namespace AutoSpex.Client.Features.Collections;

public partial class CollectionViewModel : NodeDetailViewModel
{
    private readonly IMediator _mediator;
    
    public CollectionViewModel(Node node) : base(node)
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