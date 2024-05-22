using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListNodes(NodeType Type) : IDbQuery<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class ListNodesHandler(IConnectionManager manager) : IRequestHandler<ListNodes, Result<IEnumerable<Node>>>
{
    private const string GetNodeTree =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth
            FROM Node
            WHERE ParentId is null and Type = @Type
            UNION ALL
            SELECT n.NodeId, n.ParentId, n.Type, n.Name, t.Depth + 1 as Depth
            FROM Node n
                    INNER JOIN Tree t ON n.ParentId = t.NodeId
        )

        SELECT NodeId, ParentId, Type, Name
        FROM Tree
        ORDER BY Depth, Name;
        """;

    public async Task<Result<IEnumerable<Node>>> Handle(ListNodes request,
        CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);

        var nodes = await connection.QueryAsync<Node>(GetNodeTree, new { request.Type });

        var lookup = new Dictionary<Guid, Node>();

        foreach (var node in nodes)
        {
            lookup.Add(node.NodeId, node);

            if (lookup.TryGetValue(node.ParentId, out var parent))
                parent.AddNode(node);
        }

        //Nodes at depth 0 are the root "feature" nodes, and we don't want to show those.
        var results = lookup.Values.Where(x => x.Depth == 1).AsEnumerable();
        return Result.Ok(results);
    }
}