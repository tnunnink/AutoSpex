using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetRun(Guid RunnerId) : IDbCommand<Result<Runner>>;

[UsedImplicitly]
internal class GetRunHandler(IConnectionManager manager) : IRequestHandler<GetRun, Result<Runner>>
{
    private const string GetRunner = "SELECT RunnerId, SourceId, Name FROM Runner WHERE RunnerId = @RunnerId";

    public async Task<Result<Runner>> Handle(GetRun request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var runner = await connection.QuerySingleOrDefaultAsync<Runner>(GetRunner, new {request.RunnerId});
        return runner is not null ? Result.Ok(runner) : Result.Fail($"Runner not found: '{request.RunnerId}'");
    }
}