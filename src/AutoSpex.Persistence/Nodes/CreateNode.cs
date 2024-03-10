using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateNode(Node Node) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateNodeHandler(IConnectionManager manager) : IRequestHandler<CreateNode, Result>
{
    private const string NextOrdinal =
        "SELECT coalesce(MAX(Ordinal) + 1, 0) FROM [Node] WHERE ParentId is null";

    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, NodeType, Name, Depth, Ordinal) " +
        "VALUES (@NodeId, @ParentId, @NodeType, @Name, @Depth, @Ordinal)";

    private const string InsertSpec = "INSERT INTO Spec (NodeId, Specification) VALUES (@NodeId, @Specification)";

    public async Task<Result> Handle(CreateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        using var transaction = connection.BeginTransaction();

        if (request.Node.Parent is null)
        {
            var next = await connection.QuerySingleAsync<int>(NextOrdinal, transaction);
            request.Node.UpdateOrdinal(next);
        }

        await connection.ExecuteAsync(InsertNode, request.Node, transaction);

        if (request.Node.Spec is not null)
        {
            var record = new {request.Node.NodeId, Specification = request.Node.Spec.Serialize()};
            await connection.ExecuteAsync(InsertSpec, record, transaction);    
        }
        
        transaction.Commit();
        return Result.Ok();
    }
}