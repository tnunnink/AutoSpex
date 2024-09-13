using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadNode(Guid NodeId) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class LoadNodeHandler(IConnectionManager manager) : IRequestHandler<LoadNode, Result<Node>>
{
    private const string LoadNode =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Distance
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT p.NodeId, p.ParentId, p.Type, p.Name, n.Distance + 1 as Distance
            FROM Node p
            INNER JOIN Nodes n ON n.ParentId = p.NodeId
        )

        SELECT NodeId, ParentId, Type, Name
        FROM Nodes
        ORDER BY Distance DESC;
        """;

    private const string LoadSpecs =
        "SELECT Config FROM Spec WHERE NodeId = @NodeId";

    private const string LoadVariables =
        "SELECT [VariableId], [Name], [Group], [Value] FROM Variable WHERE NodeId = @NodeId";

    public async Task<Result<Node>> Handle(LoadNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var nodes = (await connection.QueryAsync<Node>(LoadNode, new { request.NodeId })).BuildTree();
        var specs = await connection.QueryAsync<Spec>(LoadSpecs, new { request.NodeId });
        var variables = await connection.QueryAsync<Variable>(LoadVariables, new { request.NodeId });

        if (!nodes.TryGetValue(request.NodeId, out var requested))
            return Result.Fail<Node>($"Node not found: '{request.NodeId}'");

        requested.AddSpecs(specs);
        requested.AddVariables(variables);

        return Result.Ok(requested);
    }
}