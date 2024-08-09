using System.Text.Json;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetRun(Guid RunId) : IDbQuery<Result<Run>>;

[UsedImplicitly]
internal class GetRunHandler(IConnectionManager manager) : IRequestHandler<GetRun, Result<Run>>
{
    private const string GetRun =
        "SELECT RunId, Result, RanBy, RanOn FROM Run WHERE RunId = @RunId";

    private const string GetEnvironment =
        "SELECT Configuration FROM Run R JOIN Environment E on R.EnvironmentId = E.EnvironmentId WHERE RunId = @RunId";

    private const string GetOutcomes =
        "SELECT Outcomes FROM Run WHERE RunId = @RunId";

    public async Task<Result<Run>> Handle(GetRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var run = await connection.QuerySingleOrDefaultAsync<Run>(GetRun, new { request.RunId });
        if (run is null) return Result.Fail($"Run not found: {request.RunId}");

        var environment = await connection.QuerySingleOrDefaultAsync<string>(GetEnvironment, new { request.RunId });

        if (environment is not null)
            run.Environment = JsonSerializer.Deserialize<Environment>(environment)!;

        var outcomes = await connection.QuerySingleOrDefaultAsync<string>(GetOutcomes, new { request.RunId });
        if (outcomes is not null)
            run.Load(JsonSerializer.Deserialize<List<Outcome>>(outcomes));
        
        return Result.Ok(run);
    }
}