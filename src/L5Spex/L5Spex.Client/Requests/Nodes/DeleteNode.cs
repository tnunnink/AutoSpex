using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.Annotations;
using L5Spex.Client.Services;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Client.Requests.Nodes;

[PublicAPI]
public record DeleteNodeRequest(Guid NodeId) : IRequest<Result<Guid>>;

[UsedImplicitly]
public class DeleteNodeHandler : IRequestHandler<DeleteNodeRequest, Result<Guid>>
{
    private readonly IDatabaseProvider _database;

    public DeleteNodeHandler(IDatabaseProvider database)
    {
        _database = database;
    }

    public async Task<Result<Guid>> Handle(DeleteNodeRequest request, CancellationToken cancellationToken)
    {
        using var connection = _database.Connect();

        var result = await connection.ExecuteAsync("DELETE FROM Node WHERE NodeId = @NodeId", new {request.NodeId});
        
        return result == 1
            ? new Result<Guid>(request.NodeId)
            : new Result<Guid>(new Exception("Failed to delete node"));
    }
}