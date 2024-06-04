using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ResolveVariables(IEnumerable<Spec> Specs) : IDbCommand<Result>;

[UsedImplicitly]
internal class ResolveVariablesHandler(IConnectionManager manager) : IRequestHandler<ResolveVariables, Result>
{
    private const string GetSpecVariables =
        """
        WITH Tree AS (
            SELECT NodeId as SpecId, NodeId, ParentId
            FROM Node
            WHERE NodeId IN (@Ids)
            UNION ALL
            SELECT t.SpecId as SpecId, n.NodeId, n.ParentId
            FROM Node n
                     INNER JOIN Tree t ON t.ParentId = n.NodeId)

        SELECT t.SpecId, v.VariableId, v.NodeId, v.Name, v.Type, v.Data
        FROM Tree t
        JOIN Variable v ON v.NodeId = t.NodeId
        """;

    public async Task<Result> Handle(ResolveVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var lookup = new Dictionary<Guid, Dictionary<string, Variable>>();
        var ids = request.Specs.Select(s => s.SpecId).ToList();

        //Query returns a spec id and all "scoped" variables for that spec.
        //We will populate a lookup for quick access and return dummy int since the lookup is the result we need.
        await connection.QueryAsync<Guid, Variable, int>(GetSpecVariables, (specId, variable) =>
            {
                lookup.TryAdd(specId, []);
                lookup[specId].TryAdd(variable.Name, variable);
                return 1;
            },
            param: new { Ids = ids },
            splitOn: "VariableId");

        //Iterate the spec objects provided in the request and perform the lookup for matching variables.
        //If found, update the spec variable's value to "resolve" it to the current persisted state.
        foreach (var spec in request.Specs)
        {
            if (!lookup.TryGetValue(spec.SpecId, out var variables)) continue;

            foreach (var variable in spec.Variables)
                if (variables.TryGetValue(variable.Name, out var match))
                    variable.Value = match.Value;
        }

        return Result.Ok();
    }
}