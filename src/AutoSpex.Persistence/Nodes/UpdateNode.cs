using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Updates the name and documentation for the provided node if found. If not found this returns a failed result with
/// node not found. If updated, returns successfully result.
/// </summary>
/// <param name="Node">The node to update.</param>
[PublicAPI]
public record UpdateNode(Node Node) : IDbCommand<Result>;

[UsedImplicitly]
internal class UpdateNodeHandler(IConnectionManager manager) : IRequestHandler<UpdateNode, Result>
{
    private const string UpdateNode =
        "UPDATE NODE SET Name = @Name, Documentation = @Documentation WHERE NodeId = @NodeId";

    public async Task<Result> Handle(UpdateNode request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var result = await connection.ExecuteAsync(UpdateNode, request.Node);
        return Result.OkIf(result == 1, $"Node not found: {request.Node.NodeId}");
    }
}