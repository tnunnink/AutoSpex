using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteNodes(IEnumerable<Node> Nodes) : IRequest<Result>;

[UsedImplicitly]
internal class DeleteNodesHandler(IConnectionManager manager) : IRequestHandler<DeleteNodes, Result>
{
    private const string DeleteNode = "DELETE FROM Node WHERE NodeId = @NodeId";

    public async Task<Result> Handle(DeleteNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(DeleteNode, request.Nodes);
        await connection.Vacuum();
        return Result.Ok();
    }
}