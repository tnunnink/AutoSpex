using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record UpdateSource(Source Source) : IDbCommand<Result>;

[UsedImplicitly]
internal class UpdateSourceHandler(IConnectionManager manager) : IRequestHandler<UpdateSource, Result>
{
    private const string UnselectSources =
        "UPDATE Source Set IsSelected = 0 WHERE SourceId <> @SourceId";

    private const string UpdateSource =
        "UPDATE Source SET IsSelected = @IsSelected, Name = @Name, Description = @Description, " +
        "TargetType = @TargetType, TargetName = @TargetName, ExportedOn = @ExportedOn, " +
        "ExportedBy = @ExportedBy, Content = @Content " +
        "WHERE SourceId = @SourceId";

    public async Task<Result> Handle(UpdateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();

        //If the source to add is to be selected then reset all other sources first.
        if (request.Source.IsSelected)
        {
            await connection.ExecuteAsync(UnselectSources, new {request.Source.SourceId}, transaction);
        }
        
        var result = await connection.ExecuteAsync(UpdateSource, request.Source, transaction);
        
        transaction.Commit();
        
        return Result.OkIf(result == 1, $"Source not found: '{request.Source.SourceId}'");
    }
}