using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record SaveSource(Source Source) : IRequest<Result>;

[UsedImplicitly]
internal class SaveSourceHandler(IConnectionManager manager) : IRequestHandler<SaveSource, Result>
{
    private const string Exists =
        "SELECT COUNT() FROM Source WHERE SourceId = @SourceId";

    public async Task<Result> Handle(SaveSource request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var exists = await connection.QuerySingleAsync<int>(Exists, new { request.Source.SourceId });
        return exists == 0 ? Result.Fail($"Source not found: {request.Source.SourceId}") : Result.Ok();
    }
}