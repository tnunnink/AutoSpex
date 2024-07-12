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
    private const string LoadSpec =
        """
        WITH Tree AS (SELECT NodeId as SpecId, NodeId, ParentId
                      FROM Node
                      WHERE NodeId = @NodeId
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
        """;

    public async Task<Result<Spec>> Handle(GetSpec request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var specs = new Dictionary<Guid, Spec>();
        var variables = new Dictionary<Guid, Dictionary<string, Variable>>();

        //Query node, specification, and scoped variables and map each to a lookup.
        await connection.QueryAsync<Node, string, Variable?, int>(LoadSpec,
            (node, config, variable) =>
            {
                if (!specs.ContainsKey(node.NodeId))
                {
                    var spec = new Spec(node);
                    spec.Update(Spec.Deserialize(config));
                    specs.Add(node.NodeId, spec);
                }

                variables.TryAdd(node.NodeId, []);
                if (variable is not null)
                    variables[node.NodeId].TryAdd(variable.Name, variable);

                return 1;
            },
            param: new { request.NodeId },
            splitOn: "Specification,VariableId");

        //Iterate the spec objects (there should just be one) and perform the lookup for matching scoped variables.
        //If found, update the spec variable's value to "resolve" it to the current persisted state.
        foreach (var spec in specs.Values)
        {
            if (!variables.TryGetValue(spec.SpecId, out var scoped)) continue;

            foreach (var variable in spec.Variables)
            {
                if (!scoped.TryGetValue(variable.Name, out var match)) continue;
                variable.SyncTo(match);
            }
        }

        return !specs.TryGetValue(request.NodeId, out var target)
            ? Result.Fail<Spec>($"Node not found: '{request.NodeId}'")
            : Result.Ok(target);
    }
}