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
public record RenameNodeRequest(Guid NodeId, string Name) : IRequest<Result<Guid>>;

[UsedImplicitly]
public class RenameNodeHandler : IRequestHandler<RenameNodeRequest, Result<Guid>>
{
    private readonly IDatabaseProvider _database;

    public RenameNodeHandler(IDatabaseProvider database)
    {
        _database = database;
    }

    public async Task<Result<Guid>> Handle(RenameNodeRequest request, CancellationToken cancellationToken)
    {
        using var connection = _database.Connect();

        var result = await connection.ExecuteAsync("UPDATE Node SET Name = @Name WHERE NodeId = @NodeId",
            new {request.NodeId, request.Name});

        return result == 1
            ? new Result<Guid>(request.NodeId)
            : new Result<Guid>(new Exception("Failed to rename node"));
        //todo obviously need to think what is the best way to handle failed requests
    }
}