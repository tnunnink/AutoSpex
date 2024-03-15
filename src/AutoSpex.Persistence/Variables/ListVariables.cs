using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.Variables;

[PublicAPI]
public record ListVariables(Guid NodeId) : IDbQuery<Result<IEnumerable<Variable>>>;

[UsedImplicitly]
internal class ListVariablesHandler(IConnectionManager manager)
    : IRequestHandler<ListVariables, Result<IEnumerable<Variable>>>
{
    private const string Query = """
                                 WITH Hierarchy AS
                                          (SELECT NodeId, ParentId, Name, Depth
                                           FROM Node
                                           WHERE NodeId = @NodeId
                                           UNION ALL
                                           SELECT n.NodeId, n.ParentId, n.Name, n.Depth
                                           FROM Node n
                                                    INNER JOIN Hierarchy h ON n.NodeId = h.ParentId)

                                 SELECT v.VariableId, v.NodeId, h.Name as Scope, v.Name, v.Type, v.Value
                                 FROM Hierarchy h
                                 INNER JOIN Variable v on v.NodeId = h.NodeId
                                 ORDER BY Depth DESC, v.Name;
                                 """;

    public async Task<Result<IEnumerable<Variable>>> Handle(ListVariables request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var variables = (await connection.QueryAsync<Variable>(Query, new {request.NodeId})).ToList();
        
        var results = new Dictionary<string, Variable>();
        foreach (var variable in variables)
        {
            results.TryAdd(variable.Name, variable);
        }

        return Result.Ok(results.Select(r => r.Value));
    }
}