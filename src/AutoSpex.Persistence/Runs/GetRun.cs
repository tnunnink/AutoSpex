using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetRun(Guid RunId) : IDbCommand<Result<Run>>;

[UsedImplicitly]
internal class GetRunHandler(IConnectionManager manager) : IRequestHandler<GetRun, Result<Run>>
{
    private const string GetRun = "SELECT RunId, NodeId, SourceId, Name FROM Run WHERE RunId = @RunId";

    public async Task<Result<Run>> Handle(GetRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var runner = await connection.QuerySingleOrDefaultAsync<Run>(GetRun, new {request.RunId});
        return runner is not null ? Result.Ok(runner) : Result.Fail($"Runner not found: '{request.RunId}'");
    }
}