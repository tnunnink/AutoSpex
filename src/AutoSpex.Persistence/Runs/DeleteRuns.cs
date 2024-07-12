using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteRuns(IEnumerable<Guid> Ids) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteRunsHandler(IConnectionManager manager) : IRequestHandler<DeleteRuns, Result>
{
    private const string DeleteRuns = "DELETE FROM Run WHERE RunId IN @Ids";

    public async Task<Result> Handle(DeleteRuns request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var result = await connection.ExecuteAsync(DeleteRuns, new { request.Ids });
        return Result.Ok().WithSuccess($"Successfully deleted {result} runs");
    }
}