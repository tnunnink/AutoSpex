using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteNodes(IEnumerable<Guid> NodeIds) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteNodesHandler(IConnectionManager manager) : IRequestHandler<DeleteNodes, Result>
{
    public async Task<Result> Handle(DeleteNodes request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        await connection.ExecuteAsync("DELETE FROM Node WHERE NodeId IN @Ids", new {Ids = request.NodeIds});
        return Result.Ok();
    }
}