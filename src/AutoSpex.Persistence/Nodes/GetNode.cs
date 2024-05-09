using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetNode(Guid NodeId) : IDbQuery<Result<Node>>;

[UsedImplicitly]
internal class GetNodeHandler(IConnectionManager manager) : IRequestHandler<GetNode, Result<Node>>
{
    private const string GetNode = "SELECT NodeId, ParentId, NodeType, Name FROM Node WHERE NodeId = @NodeId";

    public async Task<Result<Node>> Handle(GetNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var node = await connection.QuerySingleOrDefaultAsync<Node>(GetNode, new { request.NodeId });
        return node is not null ? Result.Ok(node) : Result.Fail($"Node Not Found: {request.NodeId}");
    }
}