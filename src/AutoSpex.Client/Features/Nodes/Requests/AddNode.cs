using System;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

[PublicAPI]
public record AddNodeRequest(string Name, NodeType NodeType, Guid? ParentId = default) : IRequest<Result<Node>>;

[UsedImplicitly]
public abstract class AddNodeHandler
{
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";

    private const string GetParent = "SELECT * FROM [Node] WHERE NodeId = @ParentId";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, NodeType, Name, Depth, Ordinal, Description) " +
        "VALUES (@NodeId, @ParentId, @NodeType, @Name, @Depth, @Ordinal, @Description)";

    protected readonly IDataStoreProvider Store;

    protected AddNodeHandler(IDataStoreProvider store)
    {
        Store = store;
    }

    public async Task<Result<Node>> Handle(AddNodeRequest request, CancellationToken cancellationToken)
    {
        var connection = await Store.ConnectTo(StoreType.Project, cancellationToken);

        var ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal, new {request.ParentId, request.NodeType});

        var parentRecord = await connection.QuerySingleOrDefaultAsync(GetParent, new {request.ParentId});
        var parent = parentRecord is not null ? new Node(parentRecord) : default;

        var node = new
        {
            NodeId = Guid.NewGuid(),
            request.ParentId,
            Parent = parent,
            NodeType = request.NodeType.ToString(),
            request.Name,
            Depth = parent is not null ? parent.Depth + 1 : 0,
            Ordinal = ordinal,
            Description = string.Empty
        };

        await connection.ExecuteAsync(InsertNode, node);

        return Result.Ok(new Node(node));
    }
}