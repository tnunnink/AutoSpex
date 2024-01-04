using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

[PublicAPI]
public record AddNodeRequest(Node Node) : IRequest<Result<Node>>;

[UsedImplicitly]
public class AddNodeHandler : IRequestHandler<AddNodeRequest, Result<Node>>
{
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal) " +
        "VALUES (@NodeId, @ParentId, @Feature, @NodeType, @Name, @Depth, @Ordinal)";

    private readonly ProjectDatabase _database;

    public AddNodeHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public async Task<Result<Node>> Handle(AddNodeRequest request, CancellationToken cancellationToken)
    {
        var node = request.Node;

        using var connection = await _database.Connect(cancellationToken);

        node.Ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal, new {node.ParentId, node.NodeType});

        await connection.ExecuteAsync(InsertNode, node);

        return Result.Ok(node);
    }
}