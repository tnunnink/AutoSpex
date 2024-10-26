using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveNode(Node Node) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveNodeHandler(IConnectionManager manager) : IRequestHandler<SaveNode, Result>
{
    private const string NodeExists =
        "SELECT COUNT() FROM Node WHERE NodeId = @NodeId";

    private const string DeleteSpec =
        "DELETE FROM Spec WHERE NodeId = @NodeId";

    private const string InsertSpec =
        """
        INSERT INTO Spec ([SpecId], [NodeId], [Config])
        VALUES (@SpecId, @NodeId, @Config)
        """;

    public async Task<Result> Handle(SaveNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { request.Node.NodeId });
        if (exists != 1)
            return Result.Fail($"Node not found: {request.Node.NodeId}");

        await connection.ExecuteAsync(DeleteSpec, new { request.Node.NodeId }, transaction);

        await connection.ExecuteAsync(InsertSpec,
            new { request.Node.Spec.SpecId, request.Node.NodeId, Config = request.Node.Spec },
            transaction);

        transaction.Commit();

        return Result.Ok();
    }
}