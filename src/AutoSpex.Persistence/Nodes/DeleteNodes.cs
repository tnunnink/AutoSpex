using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteNodes(IEnumerable<Node> Nodes) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        return Nodes.Select(n => Change.For<DeleteNodes>(n.NodeId, ChangeType.Deleted, $"Deleted {n.Type} {n.Name}"));
    }
}

[UsedImplicitly]
internal class DeleteNodesHandler(IConnectionManager manager) : IRequestHandler<DeleteNodes, Result>
{
    private const string DeleteNode = "DELETE FROM Node WHERE NodeId = @NodeId";
    private const string VacuumFile = "VACUUM"; //this clears empty space or releases memory back to disc.

    public async Task<Result> Handle(DeleteNodes request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(DeleteNode, request.Nodes);
        await connection.ExecuteAsync(VacuumFile);
        return Result.Ok();
    }
}