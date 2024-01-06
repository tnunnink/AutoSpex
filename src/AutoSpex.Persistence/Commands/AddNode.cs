using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record AddNode(Node Node) : ICommand<Result<Node>>;

[UsedImplicitly]
internal class AddNodeHandler(IConnectionManager manager) : IRequestHandler<AddNode, Result<Node>>
{
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal) " +
        "VALUES (@NodeId, @ParentId, @Feature, @NodeType, @Name, @Depth, @Ordinal)";

    public async Task<Result<Node>> Handle(AddNode request, CancellationToken cancellationToken)
    {
        var node = request.Node;

        using var connection = await manager.Connect(Database.Project, cancellationToken);

        node.Ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal, new {node.ParentId, node.NodeType});

        await connection.ExecuteAsync(InsertNode, node);

        return Result.Ok(node);
    }
}