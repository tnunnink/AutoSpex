using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetNodes : IQuery<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class GetNodeTreeHandler(IConnectionManager manager)
    : IRequestHandler<GetNodes, Result<IEnumerable<Node>>>
{
    private const string Query = """
                                 WITH Tree AS
                                          (SELECT NodeId, ParentId, NodeType, Name, Depth, Ordinal
                                           FROM Node
                                           WHERE ParentId is null
                                 
                                           UNION ALL
                                 
                                           SELECT n.NodeId, n.ParentId, n.NodeType, n.Name, n.Depth, n.Ordinal
                                           FROM Node n
                                                    INNER JOIN Tree t ON n.ParentId = t.NodeId)
                                 SELECT *
                                 FROM Tree
                                 ORDER BY Depth, Ordinal;
                                 """;

    public async Task<Result<IEnumerable<Node>>> Handle(GetNodes request,
        CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);

        var records = await connection.QueryAsync<Node>(Query);

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