using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ExportNode(Guid NodeId) : IDbCommand<Result<Package>>;

[UsedImplicitly]
internal class ExportNodeHandler(IConnectionManager manager) : IRequestHandler<ExportNode, Result<Package>>
{
    private const string ListNodes =
        """
        WITH Nodes AS (SELECT NodeId, ParentId, Type, Name, 0 as Depth
                       FROM Node
                       WHERE NodeId = @NodeId
                       UNION ALL
                       SELECT c.NodeId, c.ParentId, c.Type, c.Name, n.Depth + 1 as Depth
                       FROM Node c
                                INNER JOIN Nodes n ON c.ParentId = n.NodeId)

        SELECT n.[NodeId],
               n.[ParentId],
               n.[Name],
               n.[Type],
               s.[Config],
               v.[VariableId],
               v.[Name],
               v.[Group],
               v.[Value]
        FROM Nodes n
                 LEFT JOIN Spec s ON s.NodeId = n.NodeId
                 LEFT JOIN Variable v ON v.NodeId = n.NodeId
        ORDER BY n.Depth, n.Name;
        """;

    private const string GetVersion = "SELECT Version FROM VersionInfo ORDER BY Version DESC";

    public async Task<Result<Package>> Handle(ExportNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var nodes = new Dictionary<Guid, Node>();

        await connection.QueryAsync<Node, Spec?, Variable?, Node>(ListNodes,
            (node, spec, variable) =>
            {
                nodes.TryAdd(node.NodeId, node);

                if (nodes.TryGetValue(node.ParentId, out var parent))
                    parent.AddNode(node);

                if (variable is not null)
                    node.AddVariable(variable);

                if (spec is not null)
                    node.AddSpec(spec);

                return node;
            },
            splitOn: "Config,VariableId",
            param: new { request.NodeId });

        if (!nodes.TryGetValue(request.NodeId, out var collection))
            return Result.Fail("Collection not found: {request.NodeId}");

        var version = await connection.QuerySingleAsync<long>(GetVersion);
        var package = new Package(collection, version);
        return Result.Ok(package);
    }
}