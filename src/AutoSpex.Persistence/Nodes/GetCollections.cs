using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetCollections : IDbQuery<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class GetCollectionsHandler(IConnectionManager manager)
    : IRequestHandler<GetCollections, Result<IEnumerable<Node>>>
{
    private const string GetNodeTree = """
                                       WITH Tree AS
                                                (SELECT NodeId, ParentId, NodeType, Name, 0 as Depth
                                                 FROM Node
                                                 WHERE ParentId is null
                                       
                                                 UNION ALL
                                       
                                                 SELECT n.NodeId, n.ParentId, n.NodeType, n.Name, t.Depth + 1 as Depth
                                                 FROM Node n
                                                          INNER JOIN Tree t ON n.ParentId = t.NodeId)
                                       SELECT *
                                       FROM Tree
                                       ORDER BY Depth, Name;
                                       """;

    public async Task<Result<IEnumerable<Node>>> Handle(GetCollections request,
        CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);

        var records = await connection.QueryAsync<Node>(GetNodeTree);

        var lookup = new Dictionary<Guid, Node>();

        foreach (var record in records)
        {
            lookup.Add(record.NodeId, record);

            if (lookup.TryGetValue(record.ParentId, out var parent))
                parent.AddNode(record);
        }

        var results = lookup.Values.Where(x => x.ParentId == Guid.Empty).AsEnumerable();
        return Result.Ok(results);
    }
}