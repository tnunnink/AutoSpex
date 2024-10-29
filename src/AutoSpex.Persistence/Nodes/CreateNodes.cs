using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateNodes(IEnumerable<Node> Nodes) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateNodesHandler(IConnectionManager manager) : IRequestHandler<CreateNodes, Result>
{
    private const string NodeExists =
        "SELECT COUNT() FROM Node WHERE NodeId IN @NodeIds";

    private const string InsertNode =
        """
        INSERT INTO Node (NodeId, ParentId, Type, Name, Comment)
        VALUES (@NodeId, @ParentId, @Type, @Name, @Comment)
        """;

    private const string InsertSpec =
        """
        INSERT INTO Spec ([SpecId], [NodeId], [Config])
        VALUES (@SpecId, @NodeId, @Config)
        """;

    public async Task<Result> Handle(CreateNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var ids = request.Nodes.Select(n => n.NodeId.ToString()).ToList();
        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { NodeIds = ids });
        if (exists > 0)
            return Result.Fail("Nodes with provided id already exist.");

        using var transaction = connection.BeginTransaction();

        foreach (var node in request.Nodes)
        {
            await connection.ExecuteAsync(InsertNode, node, transaction);

            await connection.ExecuteAsync(InsertSpec,
                new { node.Spec.SpecId, node.NodeId, Config = node.Spec },
                transaction);
        }

        transaction.Commit();
        return Result.Ok();
    }
}