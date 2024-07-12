using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListRuns : IDbQuery<Result<IEnumerable<Run>>>;

[UsedImplicitly]
internal class ListRunsHandler(IConnectionManager manager) : IRequestHandler<ListRuns, Result<IEnumerable<Run>>>
{
    private const string ListRuns =
        """
        SELECT RunId, Result, RanBy, RanOn, R.EnvironmentId, E.Name
        FROM Run R
            JOIN Environment E on R.EnvironmentId = E.EnvironmentId
        """;

    public async Task<Result<IEnumerable<Run>>> Handle(ListRuns request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var runs = await connection.QueryAsync<Run, Environment, Run>(ListRuns,
            (r, e) =>
            {
                r.Environment = e;
                return r;
            },
            splitOn: "EnvironmentId");

        return Result.Ok(runs);
    }
}