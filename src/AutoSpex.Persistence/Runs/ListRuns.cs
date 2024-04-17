using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

/// <summary>
/// Returns all <see cref="Runner"/> with the id and name properties populated for showing selectable list of runners.
/// </summary>
[PublicAPI]
public record ListRuns : IDbQuery<Result<IEnumerable<Run>>>;

[UsedImplicitly]
internal class ListRunHandler(IConnectionManager manager) : IRequestHandler<ListRuns, Result<IEnumerable<Run>>>
{
    private const string ListRuns = "SELECT * FROM Run ORDER BY RanOn";

    public async Task<Result<IEnumerable<Run>>> Handle(ListRuns request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var runners = await connection.QueryAsync<Run>(ListRuns);
        return Result.Ok(runners);
    }
}