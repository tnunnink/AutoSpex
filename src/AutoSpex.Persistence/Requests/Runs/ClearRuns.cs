using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ClearRuns(Guid EntityId = default) : IRequest<Result>;

[UsedImplicitly]
internal class ClearRunsHandler(IConnectionManager manager) : IRequestHandler<ClearRuns, Result>
{
    private const string ClearRuns =
        "DELETE FROM Run";

    private const string ClearRunsFor =
        "DELETE FROM Run WHERE Node LIKE '%' || @Id || '%' or Source LIKE '%' || @Id || '%'";

    public async Task<Result> Handle(ClearRuns request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        if (request.EntityId == Guid.Empty)
        {
            await connection.ExecuteAsync(ClearRuns);
        }
        else
        {
            await connection.ExecuteAsync(ClearRunsFor, new { Id = request.EntityId.ToString() });
        }

        await connection.Vacuum();
        return Result.Ok();
    }
}