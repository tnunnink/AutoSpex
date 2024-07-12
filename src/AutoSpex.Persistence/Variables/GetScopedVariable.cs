﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetScopedVariable(Guid NodeId, string Name) : IDbQuery<Result<Variable>>;

[UsedImplicitly]
internal class GetScopedVariableHandler(IConnectionManager manager)
    : IRequestHandler<GetScopedVariable, Result<Variable>>
{
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
        using var connection = await manager.Connect(cancellationToken);

        var scoped = new Dictionary<string, Variable>();

        var inherited = await connection.QueryAsync<Variable>(GetInheritedVariables, new { request.NodeId });
        
        foreach (var variable in inherited)
            scoped.TryAdd(variable.Name, variable);

        return !scoped.TryGetValue(request.Name, out var target)
            ? Result.Fail($"Variable '{request.Name}' not found in scope of requested spec.")
            : Result.Ok(target);
    }
}