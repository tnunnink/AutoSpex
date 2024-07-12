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
public record CreateNode(Node Node) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Node.NodeId;
    public string Message => $"Created new {Node.Type} with name '{Node.Name}'";
}

[UsedImplicitly]
internal class CreateNodeHandler(IConnectionManager manager) : IRequestHandler<CreateNode, Result>
{
    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, Type, Name, Comment) VALUES (@NodeId, @ParentId, @Type, @Name, @Comment)";

    private const string InsertSpec = "INSERT INTO Spec (SpecId) VALUES (@NodeId)";

    public async Task<Result> Handle(CreateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(InsertNode, request.Node, transaction);

        if (request.Node.Type == NodeType.Spec)
            await connection.ExecuteAsync(InsertSpec, new { request.Node.NodeId }, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}