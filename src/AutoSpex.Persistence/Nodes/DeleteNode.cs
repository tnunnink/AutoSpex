using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteNode(Guid NodeId) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteNodeHandler(IConnectionManager manager) : IRequestHandler<DeleteNode, Result>
{
    public async Task<Result> Handle(DeleteNode request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync("DELETE FROM Node WHERE NodeId = @NodeId", new {request.NodeId});
        return Result.Ok();
    }
}