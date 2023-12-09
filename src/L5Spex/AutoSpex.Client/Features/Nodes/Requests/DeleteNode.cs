using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

[PublicAPI]
public record DeleteNodeRequest(Guid NodeId) : IRequest<Result<Guid>>;

[UsedImplicitly]
public class DeleteNodeHandler : IRequestHandler<DeleteNodeRequest, Result<Guid>>
{
    private readonly IDataStoreProvider _store;

    public DeleteNodeHandler(IDataStoreProvider store)
    {
        _store = store;
    }

    public async Task<Result<Guid>> Handle(DeleteNodeRequest request, CancellationToken cancellationToken)
    {
        var connection = await _store.ConnectTo(StoreType.Project, cancellationToken);

        var result = await connection.ExecuteAsync("DELETE FROM Node WHERE NodeId = @NodeId", new {request.NodeId});
        
        return result == 1
            ? Result.Ok(request.NodeId)
            : Result.Fail<Guid>("Failed to delete node");
    }
}