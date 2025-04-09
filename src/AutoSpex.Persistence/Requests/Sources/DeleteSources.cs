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
    private const string DeleteReferences = "DELETE FROM Reference WHERE Scope LIKE @Name || '%'";

    public async Task<Result> Handle(DeleteSources request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        await connection.ExecuteAsync(DeleteSource, request.Sources);

        foreach (var source in request.Sources)
        {
            await connection.ExecuteAsync(DeleteReferences, new { source.Name});
        }
        
        await connection.Vacuum();
        return Result.Ok();
    }
}