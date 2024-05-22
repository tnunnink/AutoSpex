using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetNode(Guid NodeId) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class GetNodeHandler(IConnectionManager manager) : IRequestHandler<GetNode, Result<Node>>
{
    private const string GetNodes =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Distance
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId, n.Type, n.Name, t.Distance + 1 as Distance
            FROM Node n
                    INNER JOIN Tree t ON t.ParentId = n.NodeId
        )

        SELECT NodeId, ParentId, Type, Name
        FROM Tree
        ORDER BY Distance DESC;
        """;

    public async Task<Result<Node>> Handle(GetNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        
        var nodes = await connection.QueryAsync<Node>(GetNodes, new { request.NodeId });

        var lookup = new Dictionary<Guid, Node>();
        foreach (var node in nodes)
        {
            lookup.Add(node.NodeId, node);

            if (lookup.TryGetValue(node.ParentId, out var parent))
                parent.AddNode(node);
        }

        var target = lookup.GetValueOrDefault(request.NodeId);
        return target is not null ? Result.Ok(target) : Result.Fail($"Node Not Found: {request.NodeId}");
    }
}