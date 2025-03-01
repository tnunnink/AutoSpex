using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record FindNodes(Func<Node, bool> Predicate) : IRequest<IEnumerable<Node>>;

[UsedImplicitly]
internal class FindNodesHandler(IConnectionManager manager) : IRequestHandler<FindNodes, IEnumerable<Node>>
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

    public async Task<IEnumerable<Node>> Handle(FindNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var nodes = (await connection.QueryAsync<Node>(GetNodeTree)).BuildTree();
        var results = nodes.Values.Where(request.Predicate);
        return results;
    }
}