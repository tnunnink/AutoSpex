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
public record AddFolderRequest(string Name) : AddNodeRequest(Name, NodeType.Folder);

[UsedImplicitly]
public class AddFolderHandler : AddNodeHandler, IRequestHandler<AddFolderRequest, Result<Node>>
{
    public AddFolderHandler(IDataStoreProvider store) : base(store)
    {
    }

    public Task<Result<Node>> Handle(AddFolderRequest request, CancellationToken cancellationToken)
    {
        //for right now this just adds the node but later we can do other things if needed.
        //Not sure what other information a collection may contain.
        return base.Handle(request, cancellationToken);
    }
}