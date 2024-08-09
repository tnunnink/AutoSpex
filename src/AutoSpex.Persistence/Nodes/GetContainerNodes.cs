using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetContainerNodes : IDbQuery<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class GetContainerNodesHandler(IConnectionManager manager)
    : IRequestHandler<GetContainerNodes, Result<IEnumerable<Node>>>
{
    private const string GetContainerNodes =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth
            FROM Node
            WHERE ParentId is null
            UNION ALL
            SELECT n.NodeId, n.ParentId, n.Type, n.Name, t.Depth + 1 as Depth
            FROM Node n
            INNER JOIN Tree t ON n.ParentId = t.NodeId
            WHERE n.Type = 'Container'
        )

        SELECT NodeId, ParentId, Type, Name
        FROM Tree
        ORDER BY Depth, Name;
        """;

    public async Task<Result<IEnumerable<Node>>> Handle(GetContainerNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var containers = await connection.QueryAsync<Node>(GetContainerNodes);

        var lookup = new Dictionary<Guid, Node>();

        foreach (var container in containers)
        {
            lookup.Add(container.NodeId, container);

            if (lookup.TryGetValue(container.ParentId, out var parent))
                parent.AddNode(container);
        }
        
        var results = lookup.Values.OrderBy(x => x.Depth).AsEnumerable();
        return Result.Ok(results);
    }
}