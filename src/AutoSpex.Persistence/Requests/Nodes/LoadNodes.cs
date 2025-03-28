using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadNodes(IEnumerable<Guid> Ids) : IRequest<IEnumerable<Node>>;

[UsedImplicitly]
internal class LoadNodesHandler(IConnectionManager manager) : IRequestHandler<LoadNodes, IEnumerable<Node>>
{
    private const string LoadNodes =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Distance
            FROM Node
            WHERE NodeId in @Ids
            UNION ALL
            SELECT p.NodeId, p.ParentId, p.Type, p.Name, n.Distance + 1 as Distance
            FROM Node p
                     INNER JOIN Nodes n ON n.ParentId = p.NodeId
        )
        
        SELECT n.[NodeId], n.[ParentId], n.[Type], n.[Name], s.[Config]
        FROM Nodes n
        LEFT JOIN Spec s ON s.NodeId = n.NodeId
        ORDER BY n.Distance DESC, n.Name;
        """;

    public async Task<IEnumerable<Node>> Handle(LoadNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        
        var ids = request.Ids.Select(x => x.ToString()).ToHashSet();
        var nodes = new HashSet<Node>();
        var lookup = new Dictionary<Guid, Node>();

        await connection.QueryAsync<Node, Spec?, Node>(LoadNodes,
            (node, spec) =>
            {
                //Try to add or get the existing instance that is already partially configured.
                if (!lookup.TryAdd(node.NodeId, node))
                {
                    node = lookup[node.NodeId];
                }

                if (lookup.TryGetValue(node.ParentId, out var parent))
                    parent.AddNode(node);
                
                if (spec is not null)
                    node.Specify(spec);
                
                if (ids.Contains(node.NodeId.ToString()))
                    nodes.Add(node);
                
                return node;
            },
            splitOn: "Config",
            param: new { Ids = ids });

        return nodes;
    }
}