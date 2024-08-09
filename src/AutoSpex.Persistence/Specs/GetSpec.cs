using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetSpec(Guid NodeId) : IDbQuery<Result<Spec>>;

[UsedImplicitly]
internal class GetSpecHandler(IConnectionManager manager) : IRequestHandler<GetSpec, Result<Spec>>
{
    private const string GetSpec = "SELECT Specification FROM Spec WHERE SpecId = @NodeId";

    private const string GetNodes =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Distance
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT p.NodeId, p.ParentId, p.Type, p.Name, n.Distance + 1 as Distance
            FROM Node p
            INNER JOIN Nodes n ON n.ParentId = p.NodeId
        )

        SELECT n.[NodeId], n.[ParentId], n.[Name], n.[Type], v.[VariableId], v.[NodeId], v.[Name], v.[Group], v.[Value]
        FROM Nodes n
        LEFT JOIN Variable v ON v.NodeId = n.NodeId
        ORDER BY Distance DESC;
        """;

    public async Task<Result<Spec>> Handle(GetSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var config = await connection.QuerySingleOrDefaultAsync<string>(GetSpec, new { request.NodeId });

        var lookup = new Dictionary<Guid, Node>();

        //Load the full node tree and associated variables to allow the spec to resolve any references.
        await connection.QueryAsync<Node, Variable?, Node>(GetNodes,
            (node, variable) =>
            {
                lookup.TryAdd(node.NodeId, node);

                if (lookup.TryGetValue(node.ParentId, out var parent))
                    parent.AddNode(node);

                if (variable is not null)
                    node.AddVariable(variable);

                return node;
            },
            param: new { request.NodeId },
            splitOn: "VariableId");

        if (!lookup.TryGetValue(request.NodeId, out var target) || config is null)
            return Result.Fail<Spec>($"Node not found: '{request.NodeId}'");

        var spec = new Spec(target);
        spec.Update(Spec.Deserialize(config));
        return Result.Ok(spec);
    }
}