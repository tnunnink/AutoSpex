using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteSources(IEnumerable<Source> Sources) : ICommandRequest<Result>
{
    public IEnumerable<Change> GetChanges()
    {
        return Sources.Select(x =>
            Change.For<DeleteSources>(x.SourceId, ChangeType.Deleted, $"Deleted Source {x.Name}"));
    }
}

[UsedImplicitly]
internal class DeleteSourcesHandler(IConnectionManager manager) : IRequestHandler<DeleteSources, Result>
{
    private const string DeleteSource = "DELETE FROM Source WHERE SourceId = @SourceId";

    public async Task<Result> Handle(DeleteSources request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(DeleteSource, request.Sources);
        await manager.Vacuum(connection);
        return Result.Ok();
    }
}