using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record RenameNode(Node Node) : ICommand<Result>;

[UsedImplicitly]
internal class RenameNodeHandler(IConnectionManager manager) : IRequestHandler<RenameNode, Result>
{
    private const string Command = "UPDATE Node SET Name = @Name WHERE NodeId = @NodeId";
    public async Task<Result> Handle(RenameNode request, CancellationToken cancellationToken)
    {
        var connection = await manager.Connect(Database.Project, cancellationToken);
        var updated = await connection.ExecuteAsync(Command, request.Node);
        return Result.OkIf(updated == 1, $"Node not found: '{request.Node.NodeId}'");
    }
}