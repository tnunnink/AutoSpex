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
public record ListRunners : IDbQuery<Result<IEnumerable<Runner>>>;

[UsedImplicitly]
internal class ListRunnersHandler(IConnectionManager manager)
    : IRequestHandler<ListRunners, Result<IEnumerable<Runner>>>
{
    private const string ListRunners = "SELECT RunnerId, Name FROM Runner ORDER BY Name";

    public async Task<Result<IEnumerable<Runner>>> Handle(ListRunners request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var runners = await connection.QueryAsync<Runner>(ListRunners);
        return Result.Ok(runners);
    }
}