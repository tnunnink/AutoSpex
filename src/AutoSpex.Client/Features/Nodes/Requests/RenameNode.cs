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
public record RenameNodeRequest(Guid NodeId, string Name) : IRequest<Result<Guid>>;

[UsedImplicitly]
public class RenameNodeHandler : IRequestHandler<RenameNodeRequest, Result<Guid>>
{
    private readonly IDataStoreProvider _store;

    public RenameNodeHandler(IDataStoreProvider store)
    {
        _store = store;
    }

    public async Task<Result<Guid>> Handle(RenameNodeRequest request, CancellationToken cancellationToken)
    {
        var connection = await _store.ConnectTo(StoreType.Project, cancellationToken);

        var result = await connection.ExecuteAsync("UPDATE Node SET Name = @Name WHERE NodeId = @NodeId",
            new {request.NodeId, request.Name});

        return result == 1
            ? Result.Ok(request.NodeId)
            : Result.Fail("Failed to rename node");
        //todo obviously need to think what is the best way to handle failed requests
    }
}