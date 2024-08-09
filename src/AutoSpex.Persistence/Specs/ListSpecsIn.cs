using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListSpecsIn(Guid NodeId) : IDbQuery<Result<IEnumerable<Spec>>>;

[UsedImplicitly]
internal class ListSpecsInHandler(IConnectionManager manager) : IRequestHandler<ListSpecsIn, Result<IEnumerable<Spec>>>
{
    private const string ListSpecs =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId, n.Type, n.Name, t.Depth + 1 as Depth
            FROM Node n
                    INNER JOIN Tree t ON n.ParentId = t.NodeId
        )

        SELECT NodeId, ParentId, Type, Name, Element
        FROM Tree t
        LEFT JOIN Spec s on s.SpecId = t.NodeId
        ORDER BY Depth, Name;
        """;

    public async Task<Result<IEnumerable<Spec>>> Handle(ListSpecsIn request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var lookup = new Dictionary<Guid, Node>();
        var specs = new List<Spec>();

        await connection.QueryAsync<Node, Element?, Node>(ListSpecs,
            map: (node, element) =>
            {
                lookup.TryAdd(node.NodeId, node);

                if (lookup.TryGetValue(node.ParentId, out var parent))
                {
                    parent.AddNode(node);
                }

                if (node.Type == NodeType.Spec && element is not null)
                {
                    specs.Add(new Spec(node).Find(element));
                }

                return node;
            },
            splitOn: "Element",
            param: new { request.NodeId }
        );

        return Result.Ok(specs.AsEnumerable());
    }
}