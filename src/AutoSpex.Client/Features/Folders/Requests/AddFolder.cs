using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Folders;

[PublicAPI]
public record AddFolderRequest(Node Node) : IRequest<Result<Node>>;

[UsedImplicitly]
public class AddFolderHandler : IRequestHandler<AddFolderRequest, Result<Node>>
{
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";
    
    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal) " +
        "VALUES (@NodeId, @ParentId, @Feature, @NodeType, @Name, @Depth, @Ordinal)";

    private readonly ProjectDatabase _database;

    public AddFolderHandler(ProjectDatabase database)
    {
        _database = database;
    }

    public async Task<Result<Node>> Handle(AddFolderRequest request, CancellationToken cancellationToken)
    {
        var node = request.Node;
        
        using var connection = await _database.Connect(cancellationToken);
        
        node.Ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal, new { node.ParentId, node.NodeType });
        
        await connection.ExecuteAsync(InsertNode, node);

        return Result.Ok(node);
    }
}