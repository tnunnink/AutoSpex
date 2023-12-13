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

public record GetNodesRequest(NodeType NodeType) : IRequest<Result<IEnumerable<Node>>>;

[UsedImplicitly]
public class GetNodeTreeHandler : IRequestHandler<GetNodesRequest, Result<IEnumerable<Node>>>
{
    private const string Query = """
                                 WITH Tree AS
                                          (SELECT NodeId, ParentId, NodeType, Name, Depth, Ordinal
                                           FROM Node
                                           WHERE NodeType = @NodeType
                                 
                                           UNION ALL
                                 
                                           SELECT p.NodeId, p.ParentId, p.NodeType, p.Name, p.Depth, p.Ordinal
                                           FROM Node p
                                                    INNER JOIN Tree t ON p.NodeId = t.ParentId)
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

        var records = (await connection.QueryAsync(Query, new {NodeType = request.NodeType.ToString()})).ToList();

        var lookup = new Dictionary<Guid, Node>();

        foreach (var node in records.Select(r => new Node(r)))
        {
            lookup.Add(node.NodeId, node);

            if (!lookup.ContainsKey(node.ParentId))
                continue;

            var parent = lookup[node.ParentId];
            parent.Nodes.Add(node);
            node.AssignParent(parent);
        }

        var results = lookup.Values.Where(x => x.ParentId == Guid.Empty).AsEnumerable();
        return Result.Ok(results);
    }
}