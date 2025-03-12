using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateSource(Source Source) : IRequest<Result>;

[UsedImplicitly]
internal class UpdateSourceHandler(IConnectionManager manager) : IRequestHandler<UpdateSource, Result>
{
    private const string Exists = "SELECT COUNT() FROM Source WHERE SourceId = @SourceId";

    private const string UpdateSource =
        """
        UPDATE Source 
        SET TargetType = @TargetType,
            TargetName = @TargetName,
            ExportedOn = @ExportedOn,
            ExportedBy = @ExportedBy,
            Description = @Description,
            Content = @Content
        WHERE SourceId = @SourceId
        """;

    private const string DeleteReferences = "DELETE FROM Reference WHERE Scope LIKE @SourceName || '%'";
    private const string InsertReferences = "INSERT INTO Reference (Scope) VALUES (@Scope)";

    public async Task<Result> Handle(UpdateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Source.SourceId });
        if (exists == 0)
        {
            return Result.Fail($"Source not found: {request.Source.SourceId}");
        }

        var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(UpdateSource, request.Source, transaction);
        await connection.ExecuteAsync(DeleteReferences, new { SourceName = request.Source.Name }, transaction);

        foreach (var scope in request.Source.Content?.Scopes().Distinct() ?? [])
        {
            if (string.IsNullOrEmpty(scope.Name)) continue;
            var reference = string.Concat(request.Source.Name, scope.LocalPath);
            await connection.ExecuteAsync(InsertReferences, new { Scope = reference }, transaction);
        }

        transaction.Commit();
        return Result.Ok();
    }
}