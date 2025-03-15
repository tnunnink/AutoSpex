using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using L5Sharp.Core;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateSource(Source Source) : IRequest<Result>;

[UsedImplicitly]
internal class CreateSourceHandler(IConnectionManager manager) : IRequestHandler<CreateSource, Result>
{
    private const string TargetCount = "SELECT COUNT() FROM Source WHERE IsTarget = 1";

    private const string InsertSource =
        """
        INSERT INTO Source (SourceId, Name, IsTarget, TargetName, TargetType, ExportedOn, ExportedBy, Description, Content) 
        VALUES (@SourceId, @Name, @IsTarget, @TargetName, @TargetType, @ExportedOn, @ExportedBy, @Description, @Content)
        """;

    private const string InsertReferences = "INSERT INTO Reference (Scope) VALUES (@Scope)";

    public async Task<Result> Handle(CreateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);

        //Set as the target if no source is currently targeted
        var targets = await connection.QuerySingleAsync<int>(TargetCount);
        if (targets == 0)
        {
            request.Source.IsTarget = true;
        }

        using var transaction = connection.BeginTransaction();

        //Insert the source and content.
        await connection.ExecuteAsync(InsertSource, request.Source, transaction);

        //Get all scoped components and replace the controller name with the given source name.
        var name = request.Source.Name;
        var content = request.Source.Content;
        var scopes = content?.Scopes().Select(s => string.Concat(name, s.LocalPath)).Distinct() ?? [];

        foreach (var scope in scopes)
        {
            if (scope.EndsWith('/')) continue;
            await connection.ExecuteAsync(InsertReferences, new { Scope = scope }, transaction);
        }

        transaction.Commit();
        return Result.Ok();
    }
}