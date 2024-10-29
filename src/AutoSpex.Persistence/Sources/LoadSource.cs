using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadSource(Guid SourceId) : IDbQuery<Result<Source>>;

[UsedImplicitly]
internal class LoadSourceHandler(IConnectionManager manager) : IRequestHandler<LoadSource, Result<Source>>
{
    private const string GetSource =
        """
        SELECT SourceId, Name, IsTarget, TargetName, TargetType, ExportedBy, ExportedOn, Description, Content 
        FROM Source WHERE SourceId = @SourceId
        """;

    private const string GetSuppressions = "SELECT NodeId, Reason FROM Suppression WHERE SourceId = @SourceId";

    private const string GetOverrides = "SELECT NodeId, Config FROM Override WHERE SourceId = @SourceId";

    private const string GetNodes =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Distance
            FROM Node
            WHERE NodeId in (SELECT NodeId FROM Override WHERE SourceId = @SourceId)
            UNION ALL
            SELECT p.NodeId, p.ParentId, p.Type, p.Name, n.Distance + 1 as Distance
            FROM Node p
            INNER JOIN Nodes n ON n.ParentId = p.NodeId
        )

        SELECT NodeId, ParentId, Type, Name
        FROM Nodes
        ORDER BY Distance DESC, Name;
        """;

    public async Task<Result<Source>> Handle(LoadSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });
        if (source is null)
            return Result.Fail($"Source not found: {request.SourceId}");

        var suppressions = await connection.QueryAsync<Suppression>(GetSuppressions, new { request.SourceId });
        foreach (var suppression in suppressions)
            source.AddSuppression(suppression);

        var nodes = (await connection.QueryAsync<Node>(GetNodes, new { request.SourceId })).BuildTree();

        var overrides = await connection.QueryAsync<Guid, Spec, Node>(GetOverrides,
            (nodeId, spec) =>
            {
                if (!nodes.TryGetValue(nodeId, out var node))
                    throw new ArgumentException($"No node was found for override {nodeId}");

                node.Configure(spec);
                return node;
            },
            splitOn: "Config",
            param: new { request.SourceId });

        foreach (var node in overrides)
            source.AddOverride(node);

        return Result.Ok(source);
    }
}