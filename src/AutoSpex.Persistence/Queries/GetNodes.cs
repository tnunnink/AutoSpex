using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetNodes(Feature Feature) : IRequest<Result<IEnumerable<Node>>>;

[UsedImplicitly]
internal class GetNodeTreeHandler(IConnectionManager manager)
    : IRequestHandler<GetNodes, Result<IEnumerable<Node>>>
{
    private const string Query = """
                                 WITH Tree AS
                                          (SELECT NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal
                                           FROM Node
                                           WHERE ParentId is null AND Feature = @Feature
                                 
                                           UNION ALL
                                 
                                           SELECT n.NodeId, n.ParentId, n.Feature, n.NodeType, n.Name, n.Depth, n.Ordinal
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

        var records = await connection.QueryAsync<Node>(Query, new {request.Feature});

        var lookup = new Dictionary<Guid, Node>();

        foreach (var node in records)
        {
            lookup.Add(node.NodeId, node);

            if (lookup.TryGetValue(node.ParentId, out var parent))
                parent.AddNode(node);
        }

        var results = lookup.Values.Where(x => x.ParentId == Guid.Empty).AsEnumerable();
        return Result.Ok(results);
    }
}