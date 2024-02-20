using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetFullNode(Guid NodeId) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class GetFullNodeHandler(IConnectionManager manager) : IRequestHandler<GetFullNode, Result<Node>>
{
    private const string GetFullNode = """
                               WITH Nodes AS
                                        (SELECT NodeId, ParentId, NodeType, Name, Depth
                                         FROM Node
                                         WHERE NodeId = @NodeId
                                         UNION ALL
                                         SELECT n.NodeId, n.ParentId, n.NodeType, n.Name, n.Depth
                                         FROM Node n
                                                  INNER JOIN Nodes t ON n.NodeId = t.ParentId)
                               SELECT n.NodeId,
                                      n.ParentId,
                                      n.NodeType,
                                      n.Name,
                                      v.VariableId,
                                      v.Name,
                                      v.Value,
                                      v.Description,
                                      s.Specification
                               FROM Nodes n
                                        LEFT JOIN Variable v on v.NodeId = n.NodeId
                                        LEFT JOIN Spec s on s.NodeId = n.NodeId
                               ORDER BY Depth;
                               """;

    public async Task<Result<Node>> Handle(GetFullNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var nodes = new Dictionary<Guid, Node>();

        await connection.QueryAsync<Node, Variable?, string?, Node>(GetFullNode,
            (n, v, s) =>
            {
                //cache always if not already
                nodes.TryAdd(n.NodeId, n);
                
                //Get the already created instance from the cache.
                var node = nodes[n.NodeId];

                //look for parent.
                if (nodes.TryGetValue(node.ParentId, out var parent))
                    parent.AddNode(node);
                
                //Add variable if available
                if (v is not null)
                    node.AddVariable(v);

                //Exit early
                if (s is null) return node;
                
                //configure spec if available
                var spec = Spec.Deserialize(s);
                node.Configure(spec);
                return node;
            }, 
            param: new {request.NodeId},
            splitOn: "VariableId,Specification"
        );

        if (!nodes.TryGetValue(request.NodeId, out var target))
            return Result.Fail<Node>($"No node found with id '{request.NodeId}'");
        
        return Result.Ok(target);
    }
}