using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record MoveNodes(IEnumerable<Node> Nodes, Guid ParentId) : IRequest<Result>;

[UsedImplicitly]
internal class MoveNodesHandler(IConnectionManager manager) : IRequestHandler<MoveNodes, Result>
{
    private const string SetParents = "UPDATE Node Set ParentId = @ParentId WHERE NodeId IN @Ids";

    public async Task<Result> Handle(MoveNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var ids = request.Nodes.Select(n => n.NodeId.ToString());
        await connection.ExecuteAsync(SetParents, new { Ids = ids, request.ParentId });

        return Result.Ok();
    }
}