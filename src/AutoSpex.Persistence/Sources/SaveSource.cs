using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSource(Source Source) : IDbCommand<Result>, IDbLoggable
{
    public Guid NodeId => Source.SourceId;

    public string Message =>
        $"Saved Source '{Source.Name}' | Target={Source.TargetName} | Type={Source.TargetType} | Exported={Source.ExportedOn}";
}

[UsedImplicitly]
internal class SaveSourceHandler(IConnectionManager manager) : IRequestHandler<SaveSource, Result>
{
    private const string UpsertSource =
        """
        UPDATE Source
        SET TargetName = @TargetName, TargetType = @TargetType, ExportedBy = @ExportedBy, ExportedOn = @ExportedOn, Content = @Content
        WHERE SourceId = @SourceId
        """;

    public async Task<Result> Handle(SaveSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        var result = await connection.ExecuteAsync(UpsertSource, request.Source);
        return Result.OkIf(result == 1, $"Source not found: '{request.Source.SourceId}'");
    }
}