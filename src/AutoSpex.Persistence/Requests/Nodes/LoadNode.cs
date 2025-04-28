using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadNode(Guid NodeId) : IRequest<Result<Node>>;

[UsedImplicitly]
internal class LoadNodeHandler(IConnectionManager manager) : IRequestHandler<LoadNode, Result<Node>>
{
    private const string GetParents =
        """
        WITH Parents AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Distance
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId, n.Type, n.Name, p.Distance + 1 as Distance
            FROM Node n
            INNER JOIN Parents p ON p.ParentId = n.NodeId
        )

        SELECT n.NodeId, n.ParentId, n.Type, n.Name, s.Config [Spec]
        FROM Parents n
        LEFT JOIN Spec s ON s.NodeId = n.NodeId
        ORDER BY n.Distance DESC, n.Name;
        """;

    private const string GetChildren =
        """
        WITH Children AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth 
            FROM Node
            WHERE ParentId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId, n.Type, n.Name, c.Depth + 1 as Depth
            FROM Node n
            INNER JOIN Children c ON c.NodeId = n.ParentId
        )

        SELECT n.NodeId, n.ParentId, n.Type, n.Name, s.Config [Spec]
        FROM Children n
        LEFT JOIN Spec s ON s.NodeId = n.NodeId
        ORDER BY n.Depth, n.Name;
        """;

    public async Task<Result<Node>> Handle(LoadNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var parents = (await connection.QueryAsync<Node>(GetParents, new { request.NodeId })).BuildTree();
        var children = (await connection.QueryAsync<Node>(GetChildren, new { request.NodeId })).BuildTree();

        if (!parents.TryGetValue(request.NodeId, out var node))
            return Result.Fail($"Node node found: {request.NodeId}");

        foreach (var descendent in children.Values.Where(d => d.Parent is null))
            node.AddNode(descendent);

        return Result.Ok(node);
    }
}