using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveNodes(IEnumerable<Node> Nodes) : IDbCommand<Result>;

[UsedImplicitly]
internal class SaveNodesHandler(IConnectionManager manager) : IRequestHandler<SaveNodes, Result>
{
    private const string UpdateSpec = "UPDATE Spec SET [Config] = @Config WHERE NodeId = @NodeId";

    public async Task<Result> Handle(SaveNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        foreach (var node in request.Nodes)
        {
            var record = new { node.NodeId, Config = node.Spec };
            var result = await connection.ExecuteAsync(UpdateSpec, record, transaction);
            if (result != 1) return Result.Fail($"Node not found: {node.NodeId}");
        }

        transaction.Commit();
        return Result.Ok();
    }
}