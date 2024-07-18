using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadSpecs(IEnumerable<Guid> Ids) : IDbQuery<Result<IEnumerable<Spec>>>;

[UsedImplicitly]
internal class LoadSpecsHandler(IConnectionManager manager) : IRequestHandler<LoadSpecs, Result<IEnumerable<Spec>>>
{
    private const string LoadNodes =
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

        SELECT n.[NodeId], n.[ParentId], n.[Name], n.[Type], v.[VariableId], v.[NodeId], v.[Name], v.[Group], v.[Value]
        FROM Nodes n
        LEFT JOIN Variable v ON v.NodeId = n.NodeId
        ORDER BY Depth, Name;
        """;

    private const string LoadSpecs = "SELECT SpecId, Specification FROM Spec WHERE SpecId In @Ids";

    /*private const string LoadSpecs =
        """
        WITH Tree AS (SELECT NodeId as SpecId, NodeId, ParentId
                      FROM Node
                      WHERE NodeId IN @Ids
                      UNION ALL
                      SELECT t.SpecId as SpecId, n.NodeId, n.ParentId
                      FROM Node n
                               INNER JOIN Tree t ON t.ParentId = n.NodeId)

        SELECT n.[NodeId], n.[ParentId], n.[Name], n.[Type],
               s.[Specification],
               v.[VariableId], v.[NodeId], v.[Name], v.[Group], v.[Value]
        FROM Tree t
                 JOIN Node n on t.SpecId = n.NodeId
                 JOIN Spec s on t.SpecId = s.SpecId
                 LEFT JOIN Variable v ON v.NodeId = t.NodeId
        """;*/


    public async Task<Result<IEnumerable<Spec>>> Handle(LoadSpecs request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var lookup = new Dictionary<Guid, Node>();

        //Load the full node tree and associated variables to allow the spec to resolve any references.
        await connection.QueryAsync<Node, Variable?, Node>(LoadNodes,
            (n, v) =>
            {
                lookup.TryAdd(n.NodeId, n);

                if (lookup.TryGetValue(n.ParentId, out var parent))
                    parent.AddNode(n);

                if (v is not null)
                    n.AddVariable(v);

                return n;
            },
            splitOn: "VariableId");

        var specs = await connection.QueryAsync<Guid, string, Spec>(LoadSpecs,
            (id, config) =>
            {
                var node = lookup[id];
                var spec = new Spec(node);
                spec.Update(Spec.Deserialize(config));
                return spec;
            },
            splitOn: "Specification",
            param: new { Ids = request.Ids.Select(x => x.ToString()).ToList() }
        );
        
        return Result.Ok(specs);
    }
}