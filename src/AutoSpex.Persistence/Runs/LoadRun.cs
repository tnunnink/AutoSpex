using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record LoadRun(Guid RunId) : IDbQuery<Result<Run>>;

[UsedImplicitly]
internal class LoadRunHandler(IConnectionManager manager) : IRequestHandler<LoadRun, Result<Run>>
{
    private const string LoadRun =
        "SELECT RunId, Name, Node, Source, Result, RanOn, RanBy, Outcomes FROM RUN WHERE RunId = @RunId";

    public async Task<Result<Run>> Handle(LoadRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var run = await connection.QuerySingleOrDefaultAsync<Run>(LoadRun, param: new { request.RunId });
        return run is not null ? Result.Ok(run) : Result.Fail($"Run not found: {request.RunId}");
    }
}