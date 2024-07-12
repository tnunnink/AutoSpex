using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListChanges(Guid NodeId) : IDbQuery<Result<IEnumerable<ChangeLog>>>;

[UsedImplicitly]
internal class ListChangesHandler(IConnectionManager manager)
    : IRequestHandler<ListChanges, Result<IEnumerable<ChangeLog>>>
{
    private const string ListChanges =
        """
        SELECT Command, Message, ChangedOn, ChangedBy, Duration
        FROM ChangeLog
        WHERE NodeId = @NodeId
        ORDER BY ChangedOn DESC
        """;

    public async Task<Result<IEnumerable<ChangeLog>>> Handle(ListChanges request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var changes = await connection.QueryAsync<ChangeLog>(ListChanges, new { request.NodeId });
        return Result.Ok(changes);
    }
}