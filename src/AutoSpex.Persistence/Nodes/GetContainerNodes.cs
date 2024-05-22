using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetContainerNodes(NodeType Type) : IDbQuery<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class GetContainerNodesHandler(IConnectionManager manager)
    : IRequestHandler<GetContainerNodes, Result<IEnumerable<Node>>>
{
    private const string GetContainerNodes =
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

    public async Task<Result<IEnumerable<Node>>> Handle(GetContainerNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var records = await connection.QueryAsync<Node>(GetContainerNodes, new { request.Type });

        var lookup = new Dictionary<Guid, Node>();

        foreach (var record in records)
        {
            lookup.Add(record.NodeId, record);

            if (lookup.TryGetValue(record.ParentId, out var parent))
                parent.AddNode(record);
        }
        
        var results = lookup.Values.Where(x => x.Type == NodeType.Container).AsEnumerable();
        return Result.Ok(results);
    }
}