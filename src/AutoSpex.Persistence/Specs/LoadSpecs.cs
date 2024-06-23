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

        SELECT t.SpecId,
               n.Name,
               s.Specification,
               v.VariableId,
               v.NodeId,
               v.Name,
               v.Type,
               v.Data
        FROM Tree t
                 JOIN Node n on t.SpecId = n.NodeId
                 JOIN Spec s on t.SpecId = s.SpecId
                 LEFT JOIN Variable v ON v.NodeId = t.NodeId
        """;


    public async Task<Result<IEnumerable<Spec>>> Handle(LoadSpecs request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var ids = request.Ids.Select(x => x.ToString()).ToList();
        var specs = new Dictionary<Guid, Spec>();
        var variables = new Dictionary<Guid, Dictionary<string, Variable>>();

        //Query all spec, mapping the name, config, and variables out into a lookup.
        await connection.QueryAsync<Guid, string, string, Variable?, int>(LoadSpecs,
            (id, name, config, variable) =>
            {
                if (!specs.ContainsKey(id))
                {
                    var spec = new Spec(Node.Create(id, NodeType.Spec, name));
                    spec.Update(Spec.Deserialize(config));
                    specs.Add(id, spec);
                }

                variables.TryAdd(id, []);
                if (variable is not null)
                    variables[id].TryAdd(variable.Name, variable);

                return 1;
            },
            param: new { Ids = ids },
            splitOn: "Name,Specification,VariableId");

        //Iterate the spec objects and perform the lookup for matching scoped variables.
        //If found, update the spec variable's value to "resolve" it to the current persisted state.
        foreach (var spec in specs.Values)
        {
            if (!variables.TryGetValue(spec.SpecId, out var scoped)) continue;

            foreach (var variable in spec.Variables)
                if (scoped.TryGetValue(variable.Name, out var match))
                    variable.Value = match.Value;
        }

        return Result.Ok(specs.Values.AsEnumerable());
    }
}