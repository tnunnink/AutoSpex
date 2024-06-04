using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetScopedVariable(Guid Id, string Name) : IDbQuery<Result<Variable>>;

internal class GetScopedVariableHandler(IConnectionManager manager)
    : IRequestHandler<GetScopedVariable, Result<Variable>>
{
    private const string FindId =
        """
        SELECT NodeId
        FROM Node N
                 LEFT JOIN Spec S ON N.NodeId = S.SpecId
        WHERE NodeId = @Id or Specification LIKE '%' || @Id || '%'
        """;

    private const string GetInheritedVariables =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId
            FROM Node n
                     INNER JOIN Tree t ON t.ParentId = n.NodeId)

        SELECT v.*
        FROM Tree t
        JOIN Variable v ON v.NodeId = t.NodeId
        """;

    public async Task<Result<Variable>> Handle(GetScopedVariable request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var nodeId = await connection.QuerySingleOrDefaultAsync<Guid>(FindId, new { request.Id });
        if (nodeId == Guid.Empty) return Result.Fail($"No spec with provided id was found: {request.Id}");

        var scoped = new Dictionary<string, Variable>();

        var inherited = await connection.QueryAsync<Variable>(GetInheritedVariables, new { NodeId = nodeId });
        foreach (var variable in inherited)
            scoped.TryAdd(variable.Name, variable);

        //todo what about source variables? Are those visible to any given node/spec?
        /*var source = await connection.QueryAsync<Variable>(GetSourceVariables, new { NodeId = nodeId });
        foreach (var variable in source)
            scoped.TryAdd(variable.Name, variable);*/

        return !scoped.TryGetValue(request.Name, out var target)
            ? Result.Fail($"Variable '{request.Name}' not found in scope of requested spec.")
            : Result.Ok(target);
    }
}