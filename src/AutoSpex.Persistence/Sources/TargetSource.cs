using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record TargetSource(Guid Id) : IDbCommand<Result>;

[UsedImplicitly]
internal class TargetSourceHandler(IConnectionManager manager) : IRequestHandler<TargetSource, Result>
{
    private const string Exists = "SELECT COUNT() FROM Source WHERE SourceId = @Id";
    private const string ResetTargets = "UPDATE Source SET IsTarget = 0";
    private const string SetTarget = "UPDATE Source SET IsTarget = 1 WHERE SourceId = @Id";
    
    public async Task<Result> Handle(TargetSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Id });
        if (exists != 1) return Result.Fail($"source not found: {request.Id}");
        
        await connection.ExecuteAsync(ResetTargets, transaction);
        await connection.ExecuteAsync(SetTarget, new { request.Id }, transaction);
        transaction.Commit();
        
        return Result.Ok();
    }
}