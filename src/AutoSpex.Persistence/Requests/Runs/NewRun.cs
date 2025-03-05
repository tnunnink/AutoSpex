using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record NewRun(Guid NodeId, Guid SourceId = default) : IRequest<Result<Run>>;

[UsedImplicitly]
internal class NewRunHandler(IConnectionManager manager) : IRequestHandler<NewRun, Result<Run>>
{
    private const string GetNode =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth 
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT c.NodeId, c.ParentId, c.Type, c.Name, n.Depth + 1 as Depth
            FROM Node c
            INNER JOIN Nodes n ON c.ParentId = n.NodeId 
        )

        SELECT NodeId, ParentId, Type, Name
        FROM Nodes
        ORDEr BY Depth
        """;

    private const string GetSource =
        """
        SELECT SourceId, Name, TargetType, TargetName, ExportedOn, ExportedBy, Description 
        FROM Source WHERE SourceId = @SourceId
        """;

    private const string GetTargetSource =
        """
        SELECT SourceId, Name, TargetType, TargetName, ExportedOn, ExportedBy 
        FROM Source WHERE IsTarget = 1
        """;

    public async Task<Result<Run>> Handle(NewRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var nodes = (await connection.QueryAsync<Node>(GetNode, new { request.NodeId })).BuildTree();
        if (!nodes.TryGetValue(request.NodeId, out var node))
            return Result.Fail($"Node not found: {request.NodeId}");

        Source? source;

        if (request.SourceId == Guid.Empty)
        {
            source = await connection.QuerySingleOrDefaultAsync<Source>(GetTargetSource);
            if (source is null)
                return Result.Fail("No source targeted");
        }
        else
        {
            source = await connection.QuerySingleOrDefaultAsync<Source>(GetSource, new { request.SourceId });
            if (source is null)
                return Result.Fail($"Source not found: {request.SourceId}");
        }

        var run = new Run(node, source);

        return Result.Ok(run);
    }
}