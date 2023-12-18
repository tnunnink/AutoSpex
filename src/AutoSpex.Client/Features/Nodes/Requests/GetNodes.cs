using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

public record GetNodesRequest(Feature Feature) : IRequest<Result<IEnumerable<Node>>>;

[UsedImplicitly]
public class GetNodeTreeHandler : IRequestHandler<GetNodesRequest, Result<IEnumerable<Node>>>
{
    private const string Query = """
                                 WITH Tree AS
                                          (SELECT NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal
                                           FROM Node
                                           WHERE ParentId = @ParentId AND Feature = @Feature
                                 
                                           UNION ALL
                                 
                                           SELECT n.NodeId, n.ParentId, n.Feature, n.NodeType, n.Name, n.Depth, n.Ordinal
                                           FROM Node n
                                                    INNER JOIN Tree t ON n.ParentId = t.NodeId)
                                 SELECT *
                                 FROM Tree
                                 ORDER BY Depth, Ordinal;
                                 """;

    private readonly IDataStoreProvider _store;

    public GetNodeTreeHandler(IDataStoreProvider store)
    {
        _store = store;
    }

    public async Task<Result<IEnumerable<Node>>> Handle(GetNodesRequest request,
        CancellationToken cancellationToken)
    {
        var connection = await _store.ConnectTo(StoreType.Project, cancellationToken);

        var records = await connection.QueryAsync<Node>(Query,
            new { ParentId = Guid.Empty.ToString(), Feature = request.Feature.ToString() });

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