using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Inserts the provided node, and additionally inserts a default row into Spec, Source, or Run depending on the node
/// type.
/// </summary>
/// <param name="Node">The node to create.</param>
[PublicAPI]
public record CreateNode(Node Node) : IRequest<Result>;

[UsedImplicitly]
internal class CreateNodeHandler(IConnectionManager manager) : IRequestHandler<CreateNode, Result>
{
    private const string NodeExists =
        "SELECT COUNT() FROM Node WHERE NodeId = @NodeId";

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

    public async Task<Result> Handle(CreateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        /*if (request.Node.Type != NodeType.Collection && request.Node.ParentId == Guid.Empty)
            return Result.Fail("Can not save virutal node. Select a collection to which this node belongs");*/

        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { request.Node.NodeId }, transaction);
        if (exists != 0)
            return Result.Fail($"Node with id already exists: {request.Node.NodeId}");

        await connection.ExecuteAsync(InsertNode, request.Node, transaction);

        await connection.ExecuteAsync(InsertSpec,
            new { request.Node.Spec.SpecId, request.Node.NodeId, Config = request.Node.Spec },
            transaction);

        transaction.Commit();

        return Result.Ok();
    }
}