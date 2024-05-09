using AutoSpex.Engine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadNode(Guid NodeId) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class LoadNodeHandler(IConnectionManager manager) : IRequestHandler<LoadNode, Result<Node>>
{
    private const string LoadNode =
        """
        WITH Tree AS (
                SELECT NodeId, ParentId, NodeType, Name, 0 as Distance
                FROM Node
                WHERE NodeId = @NodeId
                UNION ALL
                SELECT n.NodeId, n.ParentId,NodeType, Name, n.Distance + 1 as Distance 
                FROM Node n
                INNER JOIN Tree t ON t.ParentId = n.NodeId
        )
        
        SELECT *
        FROM Tree
        ORDER BY Distance DESC;
        """;

    private const string LoadVariables = "SELECT * FROM Variable WHERE NodeId = @NodeId";
    
    private const string LoadSpec = "SELECT * FROM Spec WHERE SpecId = @NodeId";

    public async Task<Result<Node>> Handle(LoadNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        return Result.Ok();
    }
}