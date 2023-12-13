using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Services;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Specifications;

[PublicAPI]
public record AddCollectionRequest(string Name) : AddNodeRequest(Name, NodeType.Collection);

[UsedImplicitly]
public class AddCollectionHandler : AddNodeHandler, IRequestHandler<AddCollectionRequest, Result<Node>>
{
    public AddCollectionHandler(IDataStoreProvider store) : base(store)
    {
    }

    public Task<Result<Node>> Handle(AddCollectionRequest request, CancellationToken cancellationToken)
    {
        //for right now this just adds the node but later we can do other things if needed.
        //Not sure what other information a collection may contain.
        return base.Handle(request, cancellationToken);
    }
}