using AutoSpex.Engine;
using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadAllNodes : IRequest<IEnumerable<Node>>;

[UsedImplicitly]
internal class LoadAllNodesHandler(IConnectionManager manager) : IRequestHandler<LoadAllNodes, IEnumerable<Node>>
{
    private const string LoadAllNodes =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth
            FROM Node
            WHERE ParentId is null   
            UNION ALL
            SELECT c.NodeId, c.ParentId, c.Type, c.Name, n.Depth + 1 as Depth
            FROM Node c
            INNER JOIN Nodes n ON c.ParentId = n.NodeId
        )

        SELECT n.NodeId, n.ParentId, n.Type, n.Name, s.Config [Spec]
        FROM Nodes n
        JOIN Spec s on s.NodeId = n.NodeId
        ORDER BY Depth, Name;
        """;

    public async Task<IEnumerable<Node>> Handle(LoadAllNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var nodes = (await connection.QueryAsync<Node>(LoadAllNodes)).BuildTree();
        return nodes.Values.Where(n => n.Type == NodeType.Spec);
    }
}