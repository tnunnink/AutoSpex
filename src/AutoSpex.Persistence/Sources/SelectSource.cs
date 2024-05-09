using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SelectSource(Guid SourceId) : IDbCommand<Result>;

[UsedImplicitly]
public class SelectSourceHandler(IConnectionManager manager) : IRequestHandler<SelectSource, Result>
{
    private const string UnselectSources = "UPDATE Source SET IsSelected = 0 WHERE SourceId <> @SourceId";
    private const string SelectSource = "UPDATE Source SET IsSelected = 1 WHERE SourceId = @SourceId";
    
    public async Task<Result> Handle(SelectSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(UnselectSources, new { request.SourceId }, transaction);
        var result = await connection.ExecuteAsync(SelectSource, new { request.SourceId }, transaction);
        transaction.Commit();
        
        return result == 1 ? Result.Ok() : Result.Fail($"Source Not Found {request.SourceId}");
    }
}