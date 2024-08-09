using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record MoveNode(Node Node) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Node.NodeId;
    public string Message => $"Moved {Node.Type} '{Node.Name}' to parent {Node.Parent?.Name}";
}

[UsedImplicitly]
internal class MoveNodeHandler(IConnectionManager manager) : IRequestHandler<MoveNode, Result>
{
    private const string SetParent = "UPDATE Node Set ParentId = @ParentId WHERE NodeId = @NodeId";

    public async Task<Result> Handle(MoveNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(SetParent, request.Node);
        return Result.Ok();
    }
}