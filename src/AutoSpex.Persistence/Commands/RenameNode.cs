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
    public async Task<Result> Handle(RenameNode request, CancellationToken cancellationToken)
    {
        var node = request.Node;
        
        var connection = await manager.Connect(Database.Project, cancellationToken);

        await connection.ExecuteAsync("UPDATE Node SET Name = @Name WHERE NodeId = @NodeId",
            new {node.NodeId, node.Name});

        return Result.Ok();
    }
}