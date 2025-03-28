using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ExportNode(Guid NodeId) : IRequest<Result<Package>>;

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

        SELECT n.[NodeId], n.[ParentId], n.[Name], n.[Type], s.[Config]
        FROM Nodes n
        LEFT JOIN Spec s ON s.NodeId = n.NodeId
        ORDER BY n.Depth, n.Name;
        """;

    private const string GetVersion = "SELECT Version FROM VersionInfo ORDER BY Version DESC";

    public async Task<Result<Package>> Handle(ExportNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var nodes = new Dictionary<Guid, Node>();

        await connection.QueryAsync<Node, Spec?, Node>(ListNodes,
            (node, spec) =>
            {
                if (!nodes.TryAdd(node.NodeId, node))
                    node = nodes[node.NodeId];

                if (nodes.TryGetValue(node.ParentId, out var parent))
                    parent.AddNode(node);

                if (spec is not null)
                    node.Specify(spec);

                return node;
            },
            splitOn: "Config",
            param: new { request.NodeId });

        if (!nodes.TryGetValue(request.NodeId, out var collection))
            return Result.Fail("Collection not found: {request.NodeId}");

        /*var sources = nodes.Values
            .SelectMany(n => n.Spec.GetAllReferences())
            .Where(r => r.IsSource)
            .DistinctBy(r => r.Scope.Controller);*/

        var version = await connection.QueryFirstAsync<long>(GetVersion);
        var package = new Package(collection, version);
        return Result.Ok(package);
    }
}