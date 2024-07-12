using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Persistence;

[PublicAPI]
public record GetTargetEnvironment : IDbQuery<Result<Environment>>;

[UsedImplicitly]
internal class GetTargetEnvironmentHandler(IConnectionManager manager)
    : IRequestHandler<GetTargetEnvironment, Result<Environment>>
{
    private const string GetTarget =
        "SELECT EnvironmentId, Name, Comment, IsTarget FROM Environment WHERE IsTarget = 1";

    public async Task<Result<Environment>> Handle(GetTargetEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var environment = await connection.QuerySingleOrDefaultAsync<Environment>(GetTarget);
        return environment is not null ? Result.Ok(environment) : Result.Fail("No target environment found");
    }
}