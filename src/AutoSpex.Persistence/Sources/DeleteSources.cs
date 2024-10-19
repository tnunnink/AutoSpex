using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record DeleteSources(IEnumerable<Guid> Ids) : IDbCommand<Result>;

[UsedImplicitly]
internal class DeleteSourcesHandler(IConnectionManager manager) : IRequestHandler<DeleteSources, Result>
{
    private const string DeleteSources = "DELETE FROM Source WHERE SourceId IN @Ids";

    public async Task<Result> Handle(DeleteSources request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var ids = request.Ids.Select(n => n.ToString()).ToList();
        await connection.ExecuteAsync(DeleteSources, new { Ids = ids });
        return Result.Ok();
    }
}