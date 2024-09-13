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

    private const string DeleteSpecs =
        "DELETE FROM Spec WHERE NodeId = @NodeId";

    private const string DeleteVariables =
        "DELETE FROM Variable WHERE NodeId = @NodeId";

    private const string InsertSpec =
        """
        INSERT INTO Spec ([SpecId], [NodeId], [Config])
        VALUES (@SpecId, @NodeId, @Config)
        """;

    private const string InsertVariable =
        """
        INSERT INTO Variable ([VariableId], [NodeId], [Name], [Group], [Value])
        VALUES (@VariableId, @NodeId, @Name, @Group, @Value)
        """;

    public async Task<Result> Handle(SaveNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { request.Node.NodeId });
        if (exists != 1)
            return Result.Fail($"Node not found: {request.Node.NodeId}");

        await connection.ExecuteAsync(DeleteSpecs, new { request.Node.NodeId }, transaction);
        await connection.ExecuteAsync(DeleteVariables, new { request.Node.NodeId }, transaction);

        await connection.ExecuteAsync(InsertSpec,
            request.Node.Specs.Select(s => new { s.SpecId, request.Node.NodeId, Config = s }),
            transaction);

        await connection.ExecuteAsync(InsertVariable,
            request.Node.Variables.Select(v => new { v.VariableId, request.Node.NodeId, v.Name, v.Group, v.Value }),
            transaction);

        transaction.Commit();

        return Result.Ok();
    }
}