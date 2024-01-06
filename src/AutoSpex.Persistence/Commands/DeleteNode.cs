using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteNodeRequest(Guid NodeId) : IRequest<Result>;

[UsedImplicitly]
public class DeleteNodeHandler(IConnectionManager manager) : IRequestHandler<DeleteNodeRequest, Result>
{
    public async Task<Result> Handle(DeleteNodeRequest request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        await connection.ExecuteAsync("DELETE FROM Node WHERE NodeId = @NodeId", new {request.NodeId});
        return Result.Ok();
    }
}