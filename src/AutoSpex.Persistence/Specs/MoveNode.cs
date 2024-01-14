using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record MoveNode(Node Node) : ICommand<Result>;

[UsedImplicitly]
internal class MoveNodeHandler(IConnectionManager manager) : IRequestHandler<MoveNode, Result>
{
    private const string GetCurrent = "SELECT NodeId, ParentId, Depth FROM Node WHERE NodeId = @NodeId";

    private const string GetOrdinal = "SELECT Ordinal FROM Node WHERE NodeId = @NodeId";

    private const string SetPosition =
        "UPDATE Node Set ParentId = @ParentId, Depth = @Depth, Ordinal = @Ordinal WHERE NodeId = @NodeId";

    private const string UpdateOldSiblings =
        "UPDATE Node Set Ordinal = Ordinal - 1 WHERE ParentId = @ParentId AND Ordinal > @Ordinal";

    private const string UpdateNewSiblings =
        "UPDATE Node Set Ordinal = Ordinal + 1 WHERE ParentId = @ParentId AND Ordinal >= @Ordinal";

    public async Task<Result> Handle(MoveNode request, CancellationToken cancellationToken)
    {
        var node = request.Node;
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        var transaction = connection.BeginTransaction();

        var current = await connection.QuerySingleAsync<Node>(GetCurrent, request.Node, transaction);
        var ordinal = await connection.QuerySingleAsync<int>(GetOrdinal, request.Node, transaction);
        await connection.ExecuteAsync(SetPosition, node, transaction);
        await connection.ExecuteAsync(UpdateOldSiblings, new {current.ParentId, Ordinal = ordinal}, transaction);
        await connection.ExecuteAsync(UpdateNewSiblings, node, transaction);

        transaction.Commit();

        return Result.Ok();
    }
}