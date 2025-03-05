using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteSources(IEnumerable<Source> Sources) : IRequest<Result>;

[UsedImplicitly]
internal class DeleteSourcesHandler(IConnectionManager manager) : IRequestHandler<DeleteSources, Result>
{
    private const string DeleteSource = "DELETE FROM Source WHERE SourceId = @SourceId";

    public async Task<Result> Handle(DeleteSources request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(DeleteSource, request.Sources);
        await connection.Vacuum();
        return Result.Ok();
    }
}