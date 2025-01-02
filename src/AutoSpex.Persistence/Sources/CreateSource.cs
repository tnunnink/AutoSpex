using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record CreateSource(Source Source) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        yield return Change.For<CreateSource>(Source.SourceId, ChangeType.Created, $"Created Source {Source.Name}");
    }
}

[UsedImplicitly]
internal class CreateSourceHandler(IConnectionManager manager) : IRequestHandler<CreateSource, Result>
{
    private const string TargetCount = "SELECT COUNT() FROM Source WHERE IsTarget = 1";

    private const string InsertSource =
        """
        INSERT INTO Source (SourceId, Name, IsTarget, TargetName, TargetType, ExportedOn, ExportedBy, Description, Content) 
        VALUES (@SourceId, @Name, @IsTarget, @TargetName, @TargetType, @ExportedOn, @ExportedBy, @Description, @Content)
        """;

    public async Task<Result> Handle(CreateSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        using var transaction = connection.BeginTransaction();

        //Check if any sources are targeted. If not then we want to set the new source to be the target by default.
        var targets = await connection.QuerySingleAsync<int>(TargetCount, transaction);
        if (targets == 0)
            request.Source.IsTarget = true;

        await connection.ExecuteAsync(InsertSource, request.Source, transaction);

        transaction.Commit();
        return Result.Ok();
    }
}