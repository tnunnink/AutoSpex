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
public record CreateNode(Node Node) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateNodeHandler(IConnectionManager manager) : IRequestHandler<CreateNode, Result>
{
    private const string NodeExists =
        "SELECT COUNT() FROM Node WHERE NodeId = @NodeId";

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

    public async Task<Result> Handle(CreateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        if (request.Node.Type != NodeType.Collection && request.Node.ParentId == Guid.Empty)
        {
            //todo should this not be allowed here
        }

        var exists = await connection.QuerySingleAsync<int>(NodeExists, new { request.Node.NodeId }, transaction);
        if (exists != 0)
            return Result.Fail($"Node with id already exists: {request.Node.NodeId}");

        await connection.ExecuteAsync(InsertNode, request.Node, transaction);

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