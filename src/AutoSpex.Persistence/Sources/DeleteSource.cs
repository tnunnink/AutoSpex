using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteSource(Guid SourceId) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteSourceHandler(IConnectionManager manager) : IRequestHandler<DeleteSource, Result>
{
    private const string Command = "DELETE FROM Source WHERE SourceId = @SourceId";
    
    public async Task<Result> Handle(DeleteSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var result = await connection.ExecuteAsync(Command, new {request.SourceId});
        return Result.OkIf(result == 1, $"Source not found: '{request.SourceId}'");
    }
} 