using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameNode(Node Node) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        yield return Change.For<RenameNode>(Node.NodeId, ChangeType.Renamed, $"Renamed {Node.Type} to {Node.Name}");
    }
}

[UsedImplicitly]
internal class RenameNodeHandler(IConnectionManager manager) : IRequestHandler<RenameNode, Result>
{
    private const string Command = "UPDATE Node SET Name = @Name WHERE NodeId = @NodeId";

    public async Task<Result> Handle(RenameNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var updated = await connection.ExecuteAsync(Command, request.Node);
        return Result.OkIf(updated == 1, $"Node not found: '{request.Node.NodeId}'");
    }
}