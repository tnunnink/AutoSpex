using System.Data.SQLite;
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
        "INSERT INTO Source (SourceId, IsSelected, Name, Description, TargetType, TargetName, ExportedBy, ExportedOn, Content)" +
        " VALUES (@SourceId, @IsSelected, @Name, @Description, @TargetType, @TargetName, @ExportedBy, @ExportedOn, @Content)";

    /*private const string InsertValues =
        "INSERT INTO SourceValue(SourceId, Hash, Value) VALUES (@SourceId, @Hash, @Value)";*/

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

        /*var values = request.Source.GetDistinctValues()
            .Select(v => new {request.Source.SourceId, Hash = v.StableHash(), Value = v})
            .ToList();
        await connection.ExecuteAsync(InsertValues, values, transaction);*/
        
        transaction.Commit();
        return Result.Ok();
    }
}