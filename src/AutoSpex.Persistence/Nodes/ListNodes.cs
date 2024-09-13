using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListNodes : IDbQuery<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class ListNodesHandler(IConnectionManager manager) : IRequestHandler<ListNodes, Result<IEnumerable<Node>>>
{
    private const string GetNodeTree =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth
            FROM Node
            WHERE ParentId is null
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
        var connection = await manager.Connect(cancellationToken);

        var nodes = (await connection.QueryAsync<Node>(GetNodeTree)).BuildTree();

        var results = nodes.Values.Where(x => x.Depth == 0).AsEnumerable();

        return Result.Ok(results);
    }
}