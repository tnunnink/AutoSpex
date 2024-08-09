﻿using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetReferenceVariable(Reference Reference) : IDbQuery<Result<Variable>>;

[UsedImplicitly]
internal class GetReferenceVariableHandler(IConnectionManager manager)
    : IRequestHandler<GetReferenceVariable, Result<Variable>>
{
    /// <summary>
    /// Will find the node id for the spec that contains the provided reference id.
    /// </summary>
    private const string GetNodeId =
        "SELECT SpecId as NodeId FROM Spec WHERE Specification LIKE '%' || @ReferenceId || '%'";

    /// <summary>
    /// Gets all nodes from the found node id and joins the variables defined for them.
    /// We use this to build the list of scoped variables by name.
    /// </summary>
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

    public async Task<Result<Variable>> Handle(GetReferenceVariable request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var id = request.Reference.ReferenceId;

        var nodeId = await connection.QuerySingleOrDefaultAsync<Guid>(GetNodeId, new { ReferenceId = id });
        if (nodeId == Guid.Empty)
            return Result.Fail($"No node containing id was found: {id}");

        var inherited = await connection.QueryAsync<Variable>(GetInheritedVariables, new { NodeId = nodeId });
        
        var scoped = new Dictionary<string, Variable>();
        foreach (var variable in inherited)
            scoped.TryAdd(variable.Name, variable);

        return !scoped.TryGetValue(request.Reference.Name, out var target)
            ? Result.Fail($"Variable '{request.Reference.Name}' not found in the scope of requesting id: {id}")
            : Result.Ok(target);
    }
}