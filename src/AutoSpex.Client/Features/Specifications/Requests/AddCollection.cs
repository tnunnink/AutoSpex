using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Node = AutoSpex.Client.Features.Nodes.Node;

namespace AutoSpex.Client.Features.Specifications;

[PublicAPI]
public record AddCollectionRequest(string Name) : IRequest<Result<Node>>;

[UsedImplicitly]
public class AddCollectionHandler : IRequestHandler<AddCollectionRequest, Result<Node>>
{
    private const string GetNextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId = @ParentId AND NodeType = @NodeType";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Feature, NodeType, Name, Depth, Ordinal, Description) " +
        "VALUES (@NodeId, @ParentId, @Feature, @NodeType, @Name, @Depth, @Ordinal, @Description)";

    private readonly IDataStoreProvider _store;

    public AddCollectionHandler(IDataStoreProvider store)
    {
        _store = store;
    }

    public async Task<Result<Node>> Handle(AddCollectionRequest request, CancellationToken cancellationToken)
    {
        using var connection = await _store.ConnectTo(StoreType.Project, cancellationToken);

        var node = Node.SpecCollection(request.Name);
        
        node.Ordinal = await connection.QuerySingleAsync<int>(GetNextOrdinal, new { node.ParentId, node.NodeType });
        
        var result = await connection.ExecuteAsync(InsertNode, node);

        return result == 1 ? Result.Ok(node) : Result.Fail("...");
    }
}