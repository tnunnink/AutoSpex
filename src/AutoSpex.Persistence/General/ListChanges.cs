using Dapper;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence.General;

[PublicAPI]
public record ListChanges(Guid EntityId) : IRequest<IEnumerable<Change>>;

[UsedImplicitly]
internal class ListChangesHandler(IConnectionManager manager) : IRequestHandler<ListChanges, IEnumerable<Change>>
{
    private const string ListChanges =
        """
        SELECT ChangeId, EntityId, Request, ChangeType, ChangedOn, ChangedBy, Message 
        FROM Change 
        WHERE EntityId = @EntityId;
        """;

    public async Task<IEnumerable<Change>> Handle(ListChanges request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var changes = await connection.QueryAsync<Change>(ListChanges, new { request.EntityId });
        return changes;
    }
}