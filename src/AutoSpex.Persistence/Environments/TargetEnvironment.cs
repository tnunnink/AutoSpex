using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record TargetEnvironment(Guid EnvironmentId) : IDbCommand<Result>;

[UsedImplicitly]
internal class TargetEnvironmentHandler(IConnectionManager manager) : IRequestHandler<TargetEnvironment, Result>
{
    private const string Exists = "SELECT COUNT() FROM Environment WHERE EnvironmentId = @EnvironmentId";
    private const string ResetTargets = "UPDATE Environment SET IsTarget = 0";
    private const string SetTarget = "UPDATE Environment SET IsTarget = 1 WHERE EnvironmentId = @EnvironmentId";
    
    public async Task<Result> Handle(TargetEnvironment request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.EnvironmentId });
        if (exists != 1) return Result.Fail($"Environment not found: {request.EnvironmentId}");
        
        await connection.ExecuteAsync(ResetTargets, transaction);
        await connection.ExecuteAsync(SetTarget, new { request.EnvironmentId }, transaction);
        transaction.Commit();
        
        return Result.Ok();
    }
}