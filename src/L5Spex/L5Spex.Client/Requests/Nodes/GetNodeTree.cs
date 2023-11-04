using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JetBrains.Annotations;
using L5Spex.Client.Observers;
using L5Spex.Client.Services;
using LanguageExt.Common;
using MediatR;

namespace L5Spex.Client.Requests.Nodes;

public record GetNodeTreeRequest : IRequest<Result<IEnumerable<NodeObserver>>>;

[UsedImplicitly]
public class GetNodeTreeHandler : IRequestHandler<GetNodeTreeRequest, Result<IEnumerable<NodeObserver>>>
{
    private const string Query = """
                                 WITH Tree AS
                                          (SELECT NodeId, ParentId, NodeType, Name, 0 AS Level, Ordinal
                                           FROM Node
                                           WHERE ParentId IS NULL -- Start with top-level nodes
                                 
                                           UNION ALL
                                 
                                           SELECT n.NodeId, n.ParentId, n.NodeType, n.Name, t.Level + 1, n.Ordinal
                                           FROM Node n
                                                    INNER JOIN Tree t ON n.ParentId = t.NodeId)
                                 SELECT *
                                 FROM Tree
                                 ORDER BY Level, Ordinal;
                                 """;

    private readonly IDatabaseProvider _database;

    public GetNodeTreeHandler(IDatabaseProvider database)
    {
        _database = database;
    }

    public async Task<Result<IEnumerable<NodeObserver>>> Handle(GetNodeTreeRequest request,
        CancellationToken cancellationToken)
    {
        using var connection = _database.Connect();

        var nodes = (await connection.QueryAsync(Query)).ToList();

        var lookup = new Dictionary<Guid, NodeObserver>();

        foreach (var model in nodes.Select(node => new NodeObserver(node)))
        {
            lookup.Add(model.NodeId, model);

            if (!model.ParentId.HasValue || !lookup.ContainsKey(model.ParentId.Value))
                continue;

            var parent = lookup[model.ParentId.Value];
            parent.Nodes.Add(model);
        }

        var results = lookup.Values.Where(x => !x.ParentId.HasValue).ToList();
        return new Result<IEnumerable<NodeObserver>>(results);
    }
}