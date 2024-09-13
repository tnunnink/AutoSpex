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

    private const string InsertVariable =
        """
        INSERT INTO Variable ([VariableId], [NodeId], [Name], [Group], [Value])
        VALUES (@VariableId, @NodeId, @Name, @Group, @Value)
        """;

    public async Task<Result> Handle(CreateNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var ids = request.Nodes.Select(n => n.NodeId.ToString()).ToList();
        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { NodeIds = ids });
        if (exists > 0)
            return Result.Fail("Nodes with provided id already exist.");

        var transaction = connection.BeginTransaction();

        foreach (var node in request.Nodes)
        {
            await connection.ExecuteAsync(InsertNode, node, transaction);

            await connection.ExecuteAsync(InsertSpec,
                node.Specs.Select(s => new { s.SpecId, node.NodeId, Config = s }),
                transaction);

            await connection.ExecuteAsync(InsertVariable,
                node.Variables.Select(v => new { v.VariableId, node.NodeId, v.Name, v.Group, v.Value }),
                transaction);
        }

        transaction.Commit();
        return Result.Ok();
    }
}