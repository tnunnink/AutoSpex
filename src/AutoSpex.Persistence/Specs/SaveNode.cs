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
    private const string UpsertNode =
        "INSERT INTO Node(NodeId, ParentId, NodeType, Name, Depth, Ordinal) " +
        "VALUES (@NodeId, @ParentId, @NodeType, @Name, @Depth, @Ordinal) " +
        "ON CONFLICT DO UPDATE " +
        "SET NodeId = @NodeId, ParentId = @ParentId, NodeType = @NodeType, Name = @Name, Depth = @Depth, Ordinal = @Ordinal;";

    private const string UpsertSpec =
        "INSERT INTO Spec(NodeId, Specification) VALUES (@NodeId, @Specification) " +
        "ON CONFLICT DO UPDATE SET Specification = @Specification;";

    private const string UpsertVariable =
        "INSERT INTO Variable(VariableId, NodeId, Name, Value, Description) " +
        "VALUES (@VariableId, @NodeId, @Name, @Value, @Description) " +
        "ON CONFLICT DO UPDATE " +
        "SET NodeId = @NodeId, Name = @Name, Value = @Value;";

    private const string DeleteOrphanedVariables =
        "DELETE FROM Variable WHERE NodeId = @NodeId AND VariableId NOT IN @VarIds";

    public async Task<Result> Handle(SaveNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        using var transaction = connection.BeginTransaction();

        var node = request.Node;

        /*todo figure how why this won't work - await connection.ExecuteAsync(UpsertNode, new {node});*/
        await connection.ExecuteAsync(UpsertNode,
            new {node.NodeId, node.ParentId, node.NodeType, node.Name, node.Depth, node.Ordinal}, transaction);

        if (node.Spec is not null)
        {
            var record = new {node.NodeId, Specification = node.Spec.Serialize()};
            await connection.ExecuteAsync(UpsertSpec, record, transaction);
        }

        foreach (var variable in node.Variables)
        {
            var record = new {variable.VariableId, node.NodeId, variable.Name, variable.Value, variable.Description};
            await connection.ExecuteAsync(UpsertVariable, record, transaction);    
        }

        var ids = node.Variables.Select(v => v.VariableId.ToString()).ToList();
        await connection.ExecuteAsync(DeleteOrphanedVariables, new {node.NodeId, VarIds = ids}, transaction);

        transaction.Commit();

        return Result.Ok();
    }
}