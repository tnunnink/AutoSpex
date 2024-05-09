using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateSource(Source Source) : IDbCommand<Result>;

[UsedImplicitly]
internal class CreateSourceHandler(IConnectionManager manager) : IRequestHandler<CreateSource, Result>
{
    private const string UnselectSources =
        "UPDATE Source Set IsSelected = 0 WHERE SourceId <> @SourceId";

    private const string InsertSource =
        """
        INSERT INTO Source (SourceId, IsSelected, Name, Documentation, TargetType, TargetName, ExportedBy, ExportedOn, Content)
        VALUES (@SourceId, @IsSelected, @Name, @Documentation, @TargetType, @TargetName, @ExportedBy, @ExportedOn, @Content)
        """;

    public async Task<Result> Handle(CreateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        //We will always just default the new source as the "selected" source, meaning unselect the others before inserting.
        await connection.ExecuteAsync(UnselectSources, new { request.Source.SourceId }, transaction);
        await connection.ExecuteAsync(InsertSource, request.Source, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}