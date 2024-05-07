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
    private const string InsertNode =
        "INSERT INTO Node (NodeId, ParentId, NodeType, Name, Documentation) " +
        "VALUES (@NodeId, @ParentId, @NodeType, @Name, @Documentation)";

    private const string InsertSpec =
        "INSERT INTO Spec (SpecId, Element, Specification) VALUES (@SpecId, @Element, @Specification)";

    public async Task<Result> Handle(CreateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);

        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(InsertNode, request.Node, transaction);

        if (request.Node.NodeType == NodeType.Spec)
        {
            var spec = new Spec(request.Node.NodeId);
            var record = new { spec.SpecId, spec.Element, Specification = spec.Serialize() };
            await connection.ExecuteAsync(InsertSpec, record, transaction);
        }

        transaction.Commit();
        return Result.Ok();
    }
}