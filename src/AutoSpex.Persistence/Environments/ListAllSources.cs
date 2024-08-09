using AutoSpex.Engine;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Persistence;

[PublicAPI]
public record ListAllSources : IDbQuery<Result<IEnumerable<Source>>>;

[UsedImplicitly]
internal class ListAllSourcesHandler(IConnectionManager manager)
    : IRequestHandler<ListAllSources, Result<IEnumerable<Source>>>
{
    private const string GetSources = "SELECT SourceId, Name, Uri FROM Source";

    public async Task<Result<IEnumerable<Source>>> Handle(ListAllSources request, CancellationToken cancellationToken)
    {
        using var connection = await manager.Connect(cancellationToken);
        var sources = await connection.QueryAsync<Source>(GetSources);
        return Result.Ok(sources);
    }
}