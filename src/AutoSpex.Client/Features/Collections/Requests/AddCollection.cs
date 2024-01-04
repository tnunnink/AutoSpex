using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Collections;

[PublicAPI]
public record AddCollectionRequest(Node Node) : IRequest<Result<Node>>;

[UsedImplicitly]
public class AddCollectionHandler : IRequestHandler<AddCollectionRequest, Result<Node>>
{
    private readonly ProjectDatabase _database;

    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal) " +
        "VALUES (@NodeId, @ParentId, @Feature, @NodeType, @Name, @Depth, @Ordinal)";

    public AddCollectionHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public async Task<Result<Node>> Handle(AddCollectionRequest request, CancellationToken cancellationToken)
    {
        var node = request.Node;
        
        using var connection = await _database.Connect(cancellationToken);

        node.Ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal, new {node.ParentId, node.NodeType});

        await connection.ExecuteAsync(InsertNode, node);

        return Result.Ok(node);
    }
}