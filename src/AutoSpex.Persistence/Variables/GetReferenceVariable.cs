using AutoSpex.Engine;
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
        "SELECT NodeId FROM Spec WHERE Config LIKE '%' || @ReferenceId || '%'";

    /// <summary>
    /// Gets all node variables that are inherited by the provided node id.
    /// We use this to build the list of scoped variables by name.
    /// </summary>
    private const string GetInheritedVariables =
        """
        WITH Tree AS (
            SELECT NodeId, ParentId, 0 as [Distance]
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT n.NodeId, n.ParentId, t.distance + 1 [Distance]
            FROM Node n
                     INNER JOIN Tree t ON t.ParentId = n.NodeId)

        SELECT [VariableId], [Name], [Group], [Value]
        FROM Tree t
        JOIN Variable v ON v.NodeId = t.NodeId
        ORDER BY Distance
        """;

    public async Task<Result<Variable>> Handle(GetReferenceVariable request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var id = request.Reference.ReferenceId;

        var nodeId = await connection.QuerySingleOrDefaultAsync<Guid>(GetNodeId, new { ReferenceId = id });
        if (nodeId == Guid.Empty)
            return Result.Fail($"No spec containing id was found: {id}");

        var inherited = await connection.QueryAsync<Variable>(GetInheritedVariables, new { NodeId = nodeId });

        var scoped = new Dictionary<string, Variable>();

        foreach (var variable in inherited)
            scoped.TryAdd(variable.Name, variable);

        return !scoped.TryGetValue(request.Reference.Name, out var target)
            ? Result.Fail($"Variable '{request.Reference.Name}' not found in the scope of requesting reference: {id}")
            : Result.Ok(target);
    }
}