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
    private const string LoadSpecs =
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
        """;


    public async Task<Result<IEnumerable<Spec>>> Handle(LoadSpecs request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var ids = request.Ids.Select(x => x.ToString()).ToList();
        var specs = new Dictionary<Guid, Spec>();
        var variables = new Dictionary<Guid, Dictionary<string, Variable>>();

        //Query all spec, mapping the node, specification, and variables out into a lookup.
        await connection.QueryAsync<Node, string, Variable?, int>(LoadSpecs,
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
            param: new { Ids = ids },
            splitOn: "Specification,VariableId");

        //Iterate the spec objects and perform the lookup for matching scoped variables.
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

        return Result.Ok(specs.Values.AsEnumerable());
    }
}