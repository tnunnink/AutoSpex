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
    private const string InsertSource =
        """
        INSERT INTO Source (SourceId, Name, TargetName, TargetType, ExportedOn, ExportedBy, Description, Content) 
        VALUES (@SourceId, @Name, @TargetName, @TargetType, @ExportedOn, @ExportedBy, @Description, @Content)
        """;

    public async Task<Result> Handle(CreateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(InsertSource, request.Source);
        return Result.Ok();
    }
}