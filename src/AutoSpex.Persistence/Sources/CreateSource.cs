using System.Data.SQLite;
using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateSource(Source Source) : ICommand<Result>;

[UsedImplicitly]
internal class CreateSourceHandler(IConnectionManager manager) : IRequestHandler<CreateSource, Result>
{
    private const string UnselectSources = 
        "UPDATE Source Set IsSelected = 0 WHERE SourceId <> @SourceId";
    
    private const string InsertSource =
        "INSERT INTO Source (SourceId, IsSelected, Name, Description, TargetType, TargetName, ExportedBy, ExportedOn, Content)" +
        " VALUES (@SourceId, @IsSelected, @Name, @Description, @TargetType, @TargetName, @ExportedBy, @ExportedOn, @Content)";

    public async Task<Result> Handle(CreateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(Database.Project, cancellationToken);
        using var transaction = connection.BeginTransaction();
        
        //If the source to add is to be selected then reset all other sources first.
        if (request.Source.IsSelected)
        {
            await connection.ExecuteAsync(UnselectSources, new {request.Source.SourceId}, transaction);
        }
        
        await connection.ExecuteAsync(InsertSource, request.Source, transaction);
        
        transaction.Commit();
        return Result.Ok();
    }
}