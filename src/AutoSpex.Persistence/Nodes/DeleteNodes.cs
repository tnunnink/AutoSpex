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
    private const string DeleteNodes = "DELETE FROM Node WHERE NodeId IN @NodeIds";
    
    public async Task<Result> Handle(DeleteNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var ids = request.NodeIds.Select(n => n.ToString()).ToList();
        await connection.ExecuteAsync(DeleteNodes, new {NodeIds = ids});
        return Result.Ok();
    }
}