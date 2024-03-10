using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Variables;

[PublicAPI]
public record GetScopedVariables(Guid NodeId) : IDbQuery<Result<IEnumerable<Variable>>>;

[UsedImplicitly]
internal class GetScopedVariablesHandler(IConnectionManager manager)
    : IRequestHandler<GetScopedVariables, Result<IEnumerable<Variable>>>
{
    private const string GetVariables = """
                                        WITH Tree AS (
                                            SELECT NodeId, ParentId, Depth
                                            FROM Node
                                            WHERE NodeId = @NodeId
                                        
                                            UNION ALL
                                        
                                            SELECT n.NodeId, n.ParentId, n.Depth
                                            FROM Node n
                                                     INNER JOIN Tree t ON t.ParentId = n.NodeId)

                                        SELECT VariableId, Name, Value
                                        FROM Tree t
                                                 JOIN Variable v ON v.NodeId = t.NodeId
                                        ORDER BY t.Depth DESC
                                        """;

    public async Task<Result<IEnumerable<Variable>>> Handle(GetScopedVariables request,
        CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        
        var variables = await connection.QueryAsync<Variable>(GetVariables, new {request.NodeId});

        var scoped = new Dictionary<string, Variable>();
        
        foreach (var variable in variables)
            scoped.TryAdd(variable.Name, variable);

        return Result.Ok(scoped.Select(s => s.Value));
    }
}