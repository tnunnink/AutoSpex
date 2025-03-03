using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DuplicateNode(Guid NodeId, string Name) : ICommandRequest<Result<Node>>
{
    public IEnumerable<Change> GetChanges()
    {
        yield return Change.For<DuplicateNode>(NodeId, ChangeType.Created, $"Created Node {Name}");
    }
}

[UsedImplicitly]
internal class DuplicateNodeHandler(IConnectionManager manager) : IRequestHandler<DuplicateNode, Result<Node>>
{
    private const string GetNode =
        """
        WITH Nodes AS (
            SELECT NodeId, ParentId, Type, Name, 0 as Depth
            FROM Node
            WHERE NodeId = @NodeId
            UNION ALL
            SELECT c.NodeId, c.ParentId, c.Type, c.Name, n.Depth + 1 as Depth
            FROM Node c
            INNER JOIN Nodes n ON c.ParentId = n.NodeId
        )

        SELECT n.NodeId, n.ParentId, n.Name, n.Type, s.Config [Spec] 
        FROM Nodes n
        LEFT JOIN Spec s ON s.NodeId = n.NodeId
        ORDER BY n.Depth, n.Name;
        """;

    private const string InsertNode =
        """
        INSERT INTO Node (NodeId, ParentId, Type, Name, Description)
        VALUES (@NodeId, @ParentId, @Type, @Name, @Description)
        """;

    private const string InsertSpec =
        """
        INSERT INTO Spec ([SpecId], [NodeId], [Config])
        VALUES (@SpecId, @NodeId, @Config)
        """;

    public async Task<Result<Node>> Handle(DuplicateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var nodes = (await connection.QueryAsync<Node>(GetNode, new { request.NodeId })).BuildTree();

        if (!nodes.TryGetValue(request.NodeId, out var requested))
            return Result.Fail<Node>($"Node not found: '{request.NodeId}'");

        var duplicate = requested.Duplicate(request.Name);

        using var transaction = connection.BeginTransaction();

        foreach (var node in duplicate.DescendantsAndSelf())
        {
            await connection.ExecuteAsync(InsertNode, node, transaction);
            await connection.ExecuteAsync(InsertSpec,
                new { node.Spec.SpecId, node.NodeId, Config = node.Spec },
                transaction);
        }

        transaction.Commit();

        return Result.Ok(duplicate);
    }
}